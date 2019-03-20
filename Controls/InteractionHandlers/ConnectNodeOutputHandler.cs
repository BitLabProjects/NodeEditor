using NodeEditor.App.Commands;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NodeEditor.Controls.InteractionHandlers {
  class ConnectNodeOutputHandler : EditorInteractionHandlerBase {
    private readonly NodeEditorControl nodeEditor;
    private readonly Node node;
    private readonly NodeOutput nodeOutput;

    private DateTime mMouseDownTime;
    private Point2 mDragLastPoint;
    private ConnectionPath previewConnectionPath;
    private Adorner mAdorner;
    private Node toNode;
    private NodeInput toNodeInput;

    public ConnectNodeOutputHandler(NodeEditorControl nodeEditor,
                                    Node node, NodeOutput nodeOutput) {
      this.nodeEditor = nodeEditor;
      this.node = node;
      this.nodeOutput = nodeOutput;
    }

    public override bool OnMouseButtonDown(MouseButtonEditorEventArgs args) {
      if (previewConnectionPath != null) {
        // It's the confirm click, skip this event
        return true;
      }

      mDragLastPoint = args.Position;

      previewConnectionPath = new ConnectionPath();
      //previewContainer.ContentTemplate = ConnectionTemplate;
      previewConnectionPath.DataContext = new Connection(node, nodeOutput, null, null);
      previewConnectionPath.Brush = Brushes.SkyBlue;

      nodeEditor.AddPreviewElement(previewConnectionPath);

      mMouseDownTime = DateTime.Now;

      return true;
    }

    public override bool OnMouseMove(MouseEditorEventArgs args) {

      removeAdorner();

      // Find an element under the mouse with NodeInput DataContext
      var feWithNodeInputDC = VisualTreeUtils.HitTestWithDataContext<NodeInput>(nodeEditor, args.Position);
      if (feWithNodeInputDC != null) {
        toNodeInput = feWithNodeInputDC.DataContext as NodeInput;
        toNode = VisualTreeUtils.GetDataContextOnParents<Node>(feWithNodeInputDC);
        previewConnectionPath.DataContext = new Connection(node, nodeOutput, toNode, toNodeInput);

        // Create the adorner
        feWithNodeInputDC = VisualTreeUtils.GetLastParentWithDataContextOfType<NodeInput>(feWithNodeInputDC);
        mAdorner = new SimpleCircleAdorner(feWithNodeInputDC);
        var myAdornerLayer = AdornerLayer.GetAdornerLayer(feWithNodeInputDC);
        myAdornerLayer.Add(mAdorner);
      } else {
        toNode = null;
        toNodeInput = null;
        previewConnectionPath.DataContext = new Connection(node, nodeOutput, null, null);
      }

      nodeEditor.InvalidatePreview();

      return true;
    }

    private void removeAdorner() {
      if (mAdorner != null) {
        AdornerLayer.GetAdornerLayer(mAdorner).Remove(mAdorner);
        mAdorner = null;
      }
    }

    public override bool OnMouseButtonUp(MouseButtonEditorEventArgs args) {
      if (DateTime.Now.Subtract(mMouseDownTime).TotalSeconds < 1) {
        // The first interaction was a click, don't confirm immediately to support the drag
        return true;
      }

      // Launch the command
      if (toNode != null && toNodeInput != null) {
        var connection = new Connection(node, nodeOutput, toNode, toNodeInput);
        AttachedProps.GetCommandManager(nodeEditor).StartCommand(new AddConnectionCommandToken(null, connection));
      }

      removeAdorner();
      nodeEditor.EndInteraction();
      return true;
    }
  }

  // Adorners must subclass the abstract base class Adorner.
  public class SimpleCircleAdorner : Adorner {
    // Be sure to call the base class constructor.
    public SimpleCircleAdorner(UIElement adornedElement)
      : base(adornedElement) {
      b = new Border();
      //b.BorderBrush = Brushes.DarkSlateBlue;
      b.Background = Brushes.SkyBlue;
      b.BorderThickness = new Thickness(0);
      //b.CornerRadius = new CornerRadius(2);
      //b.Opacity = 0.6;
      b.Child = new TextBlock() {
        Text = "Connect",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Foreground = Brushes.Black,
        Margin = new Thickness(4, 2, 4, 2),
      };
      AddVisualChild(b);
    }

    protected override Size MeasureOverride(Size constraint) {
      b.Measure(new Size(double.PositiveInfinity, AdornedElement.DesiredSize.Height));

      return base.MeasureOverride(constraint);
    }

    protected override Size ArrangeOverride(Size finalSize) {
      b.Arrange(new Rect(this.AdornedElement.DesiredSize.Width, 
                         this.AdornedElement.DesiredSize.Height / 2 - b.DesiredSize.Height / 2, 
                         b.DesiredSize.Width, 
                         b.DesiredSize.Height));

      return base.ArrangeOverride(finalSize);
    }

    private Border b;

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) {
      return b;
    }

    // A common way to implement an adorner's rendering behavior is to override the OnRender
    // method, which is called by the layout system as part of a rendering pass.
    protected override void OnRender(DrawingContext drawingContext) {
      Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

      Pen renderPen = new Pen(new SolidColorBrush(Colors.SkyBlue), 1.5);

      drawingContext.DrawLine(renderPen, 
                              adornedElementRect.BottomLeft + new Vector(3, 0), 
                              adornedElementRect.BottomRight + new Vector(b.ActualWidth, 0));
    }
  }
}
