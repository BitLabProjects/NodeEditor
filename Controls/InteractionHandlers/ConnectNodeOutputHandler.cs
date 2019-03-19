using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace NodeEditor.Controls.InteractionHandlers {
  class ConnectNodeOutputHandler : EditorInteractionHandlerBase {
    private bool mIsDragging;
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
      mIsDragging = true;
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

      if (mAdorner != null) {
        AdornerLayer.GetAdornerLayer(mAdorner).Remove(mAdorner);
        mAdorner = null;
      }

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

    public override bool OnMouseButtonUp(MouseButtonEditorEventArgs args) {
      nodeEditor.EndInteraction();
      return true;
    }
  }
}
