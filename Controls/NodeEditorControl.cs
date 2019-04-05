using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NodeEditor.App.Commands;
using NodeEditor.Controls.InteractionHandlers;
using NodeEditor.Geometry;
using NodeEditor.Nodes;

namespace NodeEditor.Controls {
  public class NodeEditorControl : Control {
    static NodeEditorControl() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeEditorControl), new FrameworkPropertyMetadata(typeof(NodeEditorControl)));
    }

    public NodeEditorControl() {
      // TODO Remove this setting
      Viewport = new View2D(100, 100, 100, 100, 1);

      mCurrentHandler = new PanZoomHandler(this);
      mNodeDragPreviews = new Dictionary<Node, Point2>();
    }

    internal void MoveViewport(Point2 delta) {
      Viewport = new View2D(Viewport.CenterX - delta.X / Viewport.Zoom,
                            Viewport.CenterY - delta.Y / Viewport.Zoom,
                            Viewport.Width, Viewport.Height,
                            Viewport.Zoom);
      UpdateGrid();
    }

    internal void ZoomIn() {
      ApplyZoomDelta(+0.1);
    }
    internal void ZoomOut() {
      ApplyZoomDelta(-0.1);
    }
    private void ApplyZoomDelta(double delta) {
      var newZoom = Zoom + delta;
      newZoom = Math.Max(0.5, Math.Min(2, newZoom));

      var anim = new DoubleAnimation(newZoom, new Duration(TimeSpan.FromMilliseconds(300)));
      anim.EasingFunction = new CircleEase();
      BeginAnimation(NodeEditorControl.ZoomProperty, anim, HandoffBehavior.Compose);
    }

    private Grid Root;
    private ItemsControl NodesItemsControl;
    private ItemsControl ConnectionsItemsControl;
    private ConnectionsPanel PreviewPanel;
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      Root = Template.FindName("Root", this) as Grid;
      NodesItemsControl = Template.FindName("NodesItemsControl", this) as ItemsControl;
      ConnectionsItemsControl = Template.FindName("ConnectionsItemsControl", this) as ItemsControl;
      PreviewPanel = Template.FindName("PreviewPanel", this) as ConnectionsPanel;

      UpdateGrid();
    }
    internal ItemsControl GetNodesItemsControl() {
      return NodesItemsControl;
    }
    #region Panels
    private void InvalidateNodesAndConnectionsPanels() {
      VisualTreeUtils.GetItemsHost(NodesItemsControl).InvalidateMeasure();
      VisualTreeUtils.GetItemsHost(ConnectionsItemsControl).InvalidateMeasure();
    }
    #endregion

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
      var PaperSize = sizeInfo.NewSize;
      Viewport = new View2D(Viewport.CenterX, Viewport.CenterY,
                            PaperSize.Width * Zoom, PaperSize.Height * Zoom, Zoom);
      UpdateGrid();

      base.OnRenderSizeChanged(sizeInfo);
    }

    #region Interaction handlers
    private IEditorInteractionHandler mCurrentHandler;
    private bool CurrentHandlerIsPanZoom => mCurrentHandler.GetType() == typeof(PanZoomHandler);
    internal void BeginInteraction(IEditorInteractionHandler newHandler) {
      mCurrentHandler = newHandler;
    }
    internal void EndInteraction() {
      // Go back to the default handler for pan and zoom interaction
      mCurrentHandler = new PanZoomHandler(this);
      // Reset capture state when ending interaction
      if (mCaptured) {
        ReleaseMouseCapture();
        mCaptured = false;
      }
      // Also clear the preview that maybe was set up by the interaction handler
      ClearPreview();
      mNodeDragPreviews.Clear();
      InvalidateNodesAndConnectionsPanels();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e) {
      var pos = e.GetPosition(this);
      var args = new MouseWheelEditorEventArgs(new Point2(pos.X, pos.Y), e.Delta);
      e.Handled = mCurrentHandler.OnMouseWheel(args);
    }

    private bool mCaptured;
    private bool mSkipMouseMoveBecauseCapturing;
    protected override void OnMouseDown(MouseButtonEventArgs e) {
      var pos = e.GetPosition(this);
      e.Handled = mOnMouseButtonDown(pos, e.ChangedButton);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      if (mSkipMouseMoveBecauseCapturing) {
        // Calling CaptureMouse() has been observe to raise a mouse move, don't know why.
        return;
      }

      var pos = e.GetPosition(this);
      var args = new MouseEditorEventArgs(new Point2(pos.X, pos.Y));
      e.Handled = mCurrentHandler.OnMouseMove(args);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e) {
      var pos = e.GetPosition(this);
      e.Handled = mOnMouseButtonUp(pos, e.ChangedButton);
    }

    private bool mOnMouseButtonDown(Point pos, MouseButton button) {
      // Avoid capturing twice
      if (!mCaptured) {
        try {
          mSkipMouseMoveBecauseCapturing = true;
          mCaptured = CaptureMouse();
        } finally {
          mSkipMouseMoveBecauseCapturing = false;
        }
      }

      if (mCaptured) {
        var args = new MouseButtonEditorEventArgs(new Point2(pos.X, pos.Y), button);
        return mCurrentHandler.OnMouseButtonDown(args);
      }

      return false;
    }
    private bool mOnMouseButtonUp(Point pos, MouseButton button) {
      if (mCaptured) {
        mCaptured = false;
        ReleaseMouseCapture();
      }

      var args = new MouseButtonEditorEventArgs(new Point2(pos.X, pos.Y), button);
      return mCurrentHandler.OnMouseButtonUp(args);
    }
    #endregion

    #region Preview Panel
    internal void AddPreviewElement(UIElement previewElement) {
      PreviewPanel.Children.Add(previewElement);
    }
    internal void InvalidatePreview() {
      PreviewPanel.InvalidateArrange();
      foreach(UIElement x in PreviewPanel.Children) {
        x.InvalidateVisual();
      }
    }
    private void ClearPreview() {
      PreviewPanel.Children.Clear();
    }
    #endregion
    #region Preview node positions
    private Dictionary<Node, Point2> mNodeDragPreviews;
    internal Point2 GetNodePositionMaybePreview(Node node) {
      Point2 currentPreview;
      if (mNodeDragPreviews.TryGetValue(node, out currentPreview)) {
        return currentPreview;
      } else {
        return node.Position;
      }
    }
    internal void NodeDragPreview(Node node, Point2 newPosition) {
      mNodeDragPreviews[node] = newPosition;
      InvalidateNodesAndConnectionsPanels();
    }
    #endregion

    private void UpdateViewportForNewZoom() {
      var previousPaperSize = new Size(Viewport.Width / Viewport.Zoom,
                                       Viewport.Height / Viewport.Zoom);
      Viewport = new View2D(Viewport.CenterX, Viewport.CenterY,
                                       previousPaperSize.Width * Zoom, previousPaperSize.Height * Zoom, Zoom);
    }

    private void UpdateGrid() {
      if (Root == null) {
        return;
      }
      var gridVisual = Root.FindResource("GridVisual") as Border;
      var gridVisualBrush = Root.Background as VisualBrush;
      var GridSize = Zoom * 30;
      gridVisual.Width = GridSize;
      gridVisual.Height = GridSize;
      gridVisualBrush.Viewport = new Rect(0, 0, GridSize, GridSize);

      var topLeftCCS = Viewport.TransformPCSToCCS(new Point2(0, 0));
      gridVisualBrush.Transform = new TranslateTransform(-topLeftCCS.X * Zoom, -topLeftCCS.Y * Zoom);
    }

    #region Props
    public IEnumerable Nodes {
      get { return (IEnumerable)GetValue(NodesProperty); }
      set { SetValue(NodesProperty, value); }
    }
    public static readonly DependencyProperty NodesProperty =
        DependencyProperty.Register("Nodes", typeof(IEnumerable), typeof(NodeEditorControl), new PropertyMetadata(null));

    public IEnumerable Connections {
      get { return (IEnumerable)GetValue(ConnectionsProperty); }
      set { SetValue(ConnectionsProperty, value); }
    }
    public static readonly DependencyProperty ConnectionsProperty =
        DependencyProperty.Register("Connections", typeof(IEnumerable), typeof(NodeEditorControl), new PropertyMetadata(null));

    public DataTemplate NodeTemplate {
      get { return (DataTemplate)GetValue(NodeTemplateProperty); }
      set { SetValue(NodeTemplateProperty, value); }
    }
    public static readonly DependencyProperty NodeTemplateProperty =
        DependencyProperty.Register("NodeTemplate", typeof(DataTemplate), typeof(NodeEditorControl), new PropertyMetadata(null));

    public DataTemplate ConnectionTemplate {
      get { return (DataTemplate)GetValue(ConnectionTemplateProperty); }
      set { SetValue(ConnectionTemplateProperty, value); }
    }
    public static readonly DependencyProperty ConnectionTemplateProperty =
        DependencyProperty.Register("ConnectionTemplate", typeof(DataTemplate), typeof(NodeEditorControl), new PropertyMetadata(null));

    public Style NodeContainerStyle {
      get { return (Style)GetValue(NodeContainerStyleProperty); }
      set { SetValue(NodeContainerStyleProperty, value); }
    }
    public static readonly DependencyProperty NodeContainerStyleProperty =
        DependencyProperty.Register("NodeContainerStyle", typeof(Style), typeof(NodeEditorControl), new PropertyMetadata(null));

    public Style ConnectionContainerStyle {
      get { return (Style)GetValue(ConnectionContainerStyleProperty); }
      set { SetValue(ConnectionContainerStyleProperty, value); }
    }
    public static readonly DependencyProperty ConnectionContainerStyleProperty =
        DependencyProperty.Register("ConnectionContainerStyle", typeof(Style), typeof(NodeEditorControl), new PropertyMetadata(null));

    public double Zoom {
      get { return (double)GetValue(ZoomProperty); }
      set { SetValue(ZoomProperty, value); }
    }
    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register("Zoom", typeof(double), typeof(NodeEditorControl), new PropertyMetadata(1.0, Zoom_PropertyChanged));
    private static void Zoom_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var control = d as NodeEditorControl;
      control.UpdateViewportForNewZoom();
      control.UpdateGrid();
    }

    public View2D Viewport {
      get { return (View2D)GetValue(ViewportProperty); }
      set { SetValue(ViewportProperty, value); }
    }
    public static readonly DependencyProperty ViewportProperty =
        DependencyProperty.Register("Viewport", typeof(View2D), typeof(NodeEditorControl), new PropertyMetadata(new View2D(0, 0, 100, 100, 1)));
    #endregion

    #region Commands
    public ICommand BeginConnectionCommand => new DelegateCommand((object arg) => {
      if (mCaptured || !CurrentHandlerIsPanZoom) {
        // We're during an interaction, skip commands
        return;
      }

      var control = arg as FrameworkElement;
      var nodeOutput = control.DataContext as NodeOutput;
      var node = VisualTreeUtils.GetDataContextOnParents<Node>(control);
      if (nodeOutput == null || node == null) {
        // Could not determine context
        return;
      }

      if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) != 0) {
        AttachedProps.GetCommandManager(this).StartCommand(new ToggleNodeOutputIsStreamCommandToken(null, node, nodeOutput));
      } else {
        mCurrentHandler = new ConnectNodeOutputHandler(this, node, nodeOutput);
        mOnMouseButtonDown(Mouse.GetPosition(this), MouseButton.Left);
      }
    });
    public ICommand RemoveConnectionCommand => new DelegateCommand((object arg) => {
      var connection = arg as Connection;
      AttachedProps.GetCommandManager(this).StartCommand(new RemoveConnectionCommandToken(null, connection));
    });
    public ICommand EditInitialDataCommand => new DelegateCommand((object arg) => {
      var control = arg as FrameworkElement;
      var nodeInput = control.DataContext as NodeInput;
      var node = VisualTreeUtils.GetDataContextOnParents<Node>(control);
      if (nodeInput == null || node == null) {
        // Could not determine context
        return;
      }

      var inputDialog = new InputDialog("Insert the new initial data:", nodeInput.InitialData as string);
      if (inputDialog.ShowDialog() == true) {
        var newInitialData = string.IsNullOrEmpty(inputDialog.Answer) ? null : inputDialog.Answer;
        AttachedProps.GetCommandManager(this).StartCommand(new EditNodeInputInitialDataToken(null, node, nodeInput, newInitialData));
      }
    });
    public ICommand ToggleNodeOutputIsStreamCommand => new DelegateCommand((object arg) => {
      var control = arg as FrameworkElement;
      var nodeOutput = control.DataContext as NodeOutput;
      var node = VisualTreeUtils.GetDataContextOnParents<Node>(control);
      if (nodeOutput == null || node == null) {
        // Could not determine context
        return;
      }

      AttachedProps.GetCommandManager(this).StartCommand(new ToggleNodeOutputIsStreamCommandToken(null, node, nodeOutput));
    });
    public ICommand RemoveNodeCommand => new DelegateCommand((object arg) => {
      var control = arg as FrameworkElement;
      var node = control.DataContext as Node;
      if (node == null) {
        // Could not determine context
        return;
      }

      AttachedProps.GetCommandManager(this).StartCommand(new RemoveNodeCommandToken(null, node));
    });
    #endregion
  }
}
