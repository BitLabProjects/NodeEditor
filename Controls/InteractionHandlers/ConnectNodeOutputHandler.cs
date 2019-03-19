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
    private Point2 mDragLastPoint;
    private ConnectionPath previewConnectionPath;
    private Adorner mAdorner;

    private readonly NodeEditorControl nodeEditor;
    private readonly Node node;
    private readonly NodeOutput nodeOutput;
    public ConnectNodeOutputHandler(NodeEditorControl nodeEditor,
                                    Node node, NodeOutput nodeOutput) {
      this.nodeEditor = nodeEditor;
      this.node = node;
      this.nodeOutput = nodeOutput;
    }

    public override bool OnMouseButtonDown(MouseButtonEditorEventArgs args) {
      mDragLastPoint = args.Position;

      previewConnectionPath = new ConnectionPath();
      //previewContainer.ContentTemplate = ConnectionTemplate;
      previewConnectionPath.DataContext = new Connection(node, nodeOutput, null, null);
      previewConnectionPath.Brush = Brushes.CadetBlue;

      nodeEditor.AddPreviewElement(previewConnectionPath);

      return true;
    }

    public override bool OnMouseMove(MouseEditorEventArgs args) {
      nodeEditor.InvalidatePreview();

      removeAdorner();

      // Find an element under the mouse with NodeInput DataContext
      var feWithNodeInputDC = VisualTreeUtils.HitTestWithDataContext<NodeInput>(nodeEditor, args.Position);
      if (feWithNodeInputDC != null) {
        feWithNodeInputDC = VisualTreeUtils.GetLastParentWithDataContextOfType<NodeInput>(feWithNodeInputDC);
        mAdorner = new SimpleCircleAdorner(feWithNodeInputDC);
        var myAdornerLayer = AdornerLayer.GetAdornerLayer(feWithNodeInputDC);
        myAdornerLayer.Add(mAdorner);
      }

      return true;
    }

    private void removeAdorner() {
      if (mAdorner != null) {
        AdornerLayer.GetAdornerLayer(mAdorner).Remove(mAdorner);
        mAdorner = null;
      }
    }

    public override bool OnMouseButtonUp(MouseButtonEditorEventArgs args) {
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
      b = new Button();
      b.Width = 50;
      b.Height = 30;
      b.Content = new TextBlock() { Text = "Ciao!" };
      AddVisualChild(b);
    }

    private Button b;

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) {
      return b;
    }

    // A common way to implement an adorner's rendering behavior is to override the OnRender
    // method, which is called by the layout system as part of a rendering pass.
    protected override void OnRender(DrawingContext drawingContext) {
      Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

      // Some arbitrary drawing implements.
      SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
      renderBrush.Opacity = 0.2;
      Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
      double renderRadius = 5.0;

      // Draw a circle at each corner.
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
      drawingContext.DrawLine(renderPen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight);
    }
  }
}
