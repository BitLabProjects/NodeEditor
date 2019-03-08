using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NodeEditor.Geometry;

namespace NodeEditor.Controls {
  public class NodeEditorControl: Control {
    static NodeEditorControl() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeEditorControl), new FrameworkPropertyMetadata(typeof(NodeEditorControl)));
    }

    public NodeEditorControl() {
      mIsDragging = false;

      // TODO Remove this setting
      Viewport = new View2D(100, 100, 100, 100, 1);
    }

    private Grid Root;
    private ItemsControl NodesItemsControl;
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      Root = Template.FindName("Root", this) as Grid;
      NodesItemsControl = Template.FindName("NodesItemsControl", this) as ItemsControl;

      UpdateGrid();
    }
    internal ItemsControl GetNodesItemsControl() {
      return NodesItemsControl;
    }
    internal CartesianPanel GetNodesCartesianPanel() {
      if (NodesItemsControl == null) {
        return null;
      }
      return typeof(ItemsControl).InvokeMember("ItemsHost",
                                               BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance,
                                               null, NodesItemsControl, null) as CartesianPanel;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
      var PaperSize = sizeInfo.NewSize;
      Viewport = new View2D(Viewport.CenterX, Viewport.CenterY,
                                       PaperSize.Width * Zoom, PaperSize.Height * Zoom, Zoom);
      UpdateGrid();

      base.OnRenderSizeChanged(sizeInfo);
    }

    #region Mouse handling
    protected override void OnMouseWheel(MouseWheelEventArgs e) {
      e.Handled = true;

      var newZoom = Zoom + e.Delta / 1200.0;
      newZoom = Math.Max(0.5, Math.Min(2, newZoom));

      var anim = new DoubleAnimation(newZoom, new Duration(TimeSpan.FromMilliseconds(300)));
      anim.EasingFunction = new CircleEase();
      BeginAnimation(NodeEditorControl.ZoomProperty, anim, HandoffBehavior.Compose);
    }

    private bool mIsDragging;
    private Point mDragLastPoint;
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
      if (CaptureMouse()) {
        mIsDragging = true;
        mDragLastPoint = e.GetPosition(this);
        e.Handled = true;
      }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      if (mIsDragging) {
        var newPoint = e.GetPosition(this);
        var delta = newPoint - mDragLastPoint;
        mDragLastPoint = newPoint;

        Viewport = new View2D(Viewport.CenterX - delta.X / Viewport.Zoom, Viewport.CenterY - delta.Y / Viewport.Zoom,
                                         Viewport.Width, Viewport.Height,
                                         Viewport.Zoom);

        UpdateGrid();
        e.Handled = true;
      }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
      if (mIsDragging) {
        mIsDragging = false;
        ReleaseMouseCapture();
        e.Handled = true;
      }
    }
    #endregion

    private void UpdateViewportForNewZoom() {
      var previousPaperSize = new Size(Viewport.Width / Viewport.Zoom,
                                       Viewport.Height / Viewport.Zoom);
      Viewport = new View2D(Viewport.CenterX, Viewport.CenterY,
                                       previousPaperSize.Width * Zoom, previousPaperSize.Height * Zoom, Zoom);
    }

    private void UpdateGrid() {
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
  }
}
