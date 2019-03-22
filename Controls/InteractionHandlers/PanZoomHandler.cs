using NodeEditor.App.Commands;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeEditor.Controls.InteractionHandlers {
  class PanZoomHandler: EditorInteractionHandlerBase {
    private bool mIsDragging;
    private Point2 mDragLastPoint;

    private readonly NodeEditorControl nodeEditor;
    public PanZoomHandler(NodeEditorControl nodeEditor) {
      this.nodeEditor = nodeEditor;
    }

    public override bool OnMouseButtonDown(MouseButtonEditorEventArgs args) {
      mIsDragging = true;
      mDragLastPoint = args.Position;

      var mNodeFEToDrag = VisualTreeUtils.HitTestWithDataContext<Node>(nodeEditor, args.Position);
      if (mNodeFEToDrag != null) {
        var node = (Node)mNodeFEToDrag.DataContext;
        nodeEditor.BeginInteraction(new MoveNodeHandler(nodeEditor, node, mDragLastPoint));
      }

      return true;
    }

    public override bool OnMouseMove(MouseEditorEventArgs args) {
      if (mIsDragging) {
        var delta = args.Position - mDragLastPoint;
        mDragLastPoint = args.Position;

        nodeEditor.MoveViewport(delta);

        return true;
      }
      return false;
    }

    public override bool OnMouseButtonUp(MouseButtonEditorEventArgs args) {
      mIsDragging = false;
      return true;
    }

    public override bool OnMouseWheel(MouseWheelEditorEventArgs args) {
      if (args.Delta > 0) {
        nodeEditor.ZoomIn();
      } else {
        nodeEditor.ZoomOut();
      }
      return true;
    }
  }
}
