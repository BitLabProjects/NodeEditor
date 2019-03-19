﻿using NodeEditor.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NodeEditor.Controls.InteractionHandlers {
  class MouseEditorEventArgs {
    public readonly Point2 Position;
    public MouseEditorEventArgs(Point2 position) {
      this.Position = position;
    }
  }

  class MouseButtonEditorEventArgs: MouseEditorEventArgs {
    public readonly MouseButton Button;
    public MouseButtonEditorEventArgs(Point2 position, MouseButton button) : base(position) {
      this.Button = button;
    }
  }

  interface IEditorInteractionHandler {
    bool OnMouseButtonDown(MouseButtonEditorEventArgs args);
    bool OnMouseMove(MouseEditorEventArgs args);
    bool OnMouseButtonUp(MouseButtonEditorEventArgs args);

    bool OnMouseDragStart(MouseButtonEditorEventArgs args);
  }
}
