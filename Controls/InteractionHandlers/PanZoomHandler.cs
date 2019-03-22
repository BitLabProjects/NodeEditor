using NodeEditor.App.Commands;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NodeEditor.Controls.InteractionHandlers {
  class PanZoomHandler: EditorInteractionHandlerBase {
    private bool mIsDragging;
    private Point2 mDragLastPoint;

    //
    private bool mIsRightButtonDown;

    private readonly NodeEditorControl nodeEditor;
    public PanZoomHandler(NodeEditorControl nodeEditor) {
      this.nodeEditor = nodeEditor;
    }

    public override bool OnMouseButtonDown(MouseButtonEditorEventArgs args) {
      if (args.Button == MouseButton.Left) {
        mIsDragging = true;
        mDragLastPoint = args.Position;

        var mNodeFEToDrag = VisualTreeUtils.HitTestWithDataContext<Node>(nodeEditor, args.Position);
        if (mNodeFEToDrag != null) {
          var node = (Node)mNodeFEToDrag.DataContext;
          nodeEditor.BeginInteraction(new MoveNodeHandler(nodeEditor, node, mDragLastPoint));
        }
      } else if (args.Button == MouseButton.Right) {
        mIsRightButtonDown = true;
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
      if (args.Button == MouseButton.Left) {
        mIsDragging = false;
      } else if (args.Button == MouseButton.Right) {
        mIsRightButtonDown = false;
      }
      return true;
    }

    public override bool OnMouseWheel(MouseWheelEditorEventArgs args) {
      if (mIsRightButtonDown) {
        var isUndoNotRedo = (args.Delta < 0);
        AttachedProps.GetCommandManager(nodeEditor).StartCommand(new UndoRedoCommandToken(isUndoNotRedo));
      } else {
        if (args.Delta > 0) {
          nodeEditor.ZoomIn();
        } else {
          nodeEditor.ZoomOut();
        }
      }
      return true;
    }
  }
}
