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
  class MoveNodeHandler : EditorInteractionHandlerBase {
    private readonly NodeEditorControl nodeEditor;
    private readonly Node mNodeToDrag;
    private Point2 mDragLastPoint;
    private Point2 mNewNodePosition;

    public MoveNodeHandler(NodeEditorControl nodeEditor,
                           Node nodeToDrag,
                           Point2 dragStartPoint) {
      this.nodeEditor = nodeEditor;
      this.mNodeToDrag = nodeToDrag;
      this.mDragLastPoint = dragStartPoint;
      mNewNodePosition = nodeToDrag.Position;
    }

    public override bool OnMouseMove(MouseEditorEventArgs args) {
      var delta = args.Position - mDragLastPoint;
      mDragLastPoint = args.Position;

      mNewNodePosition = mNewNodePosition + delta / nodeEditor.Zoom;
      nodeEditor.NodeDragPreview(mNodeToDrag, mNewNodePosition);

      return true;
    }

    public override bool OnMouseButtonUp(MouseButtonEditorEventArgs args) {
      nodeEditor.EndInteraction();
      AttachedProps.GetCommandManager(nodeEditor).StartCommand(new MoveNodeCommandToken(null, mNodeToDrag, mNewNodePosition));
      return true;
    }
  }
}
