using NodeEditor.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  }
}
