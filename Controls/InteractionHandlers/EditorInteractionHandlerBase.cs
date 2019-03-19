using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Controls.InteractionHandlers {
  class EditorInteractionHandlerBase : IEditorInteractionHandler {
    public virtual bool OnMouseButtonDown(MouseButtonEditorEventArgs args) {
      return false;
    }

    public virtual bool OnMouseButtonUp(MouseButtonEditorEventArgs args) {
      return false;
    }

    public virtual bool OnMouseDragStart(MouseButtonEditorEventArgs args) {
      return false;
    }

    public virtual bool OnMouseMove(MouseEditorEventArgs args) {
      return false;
    }
  }
}
