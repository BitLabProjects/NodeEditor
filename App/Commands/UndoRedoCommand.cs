using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class UndoRedoCommandToken: ICommandToken {
    public readonly bool IsUndoNotRedo;
    public UndoRedoCommandToken(bool isUndoNotRedo) {
      this.IsUndoNotRedo = isUndoNotRedo;
    }
  }

  class UndoRedoCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public UndoRedoCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as UndoRedoCommandToken;

      if (token.IsUndoNotRedo) {
        mApp.Undo();
      } else {
        mApp.Redo();
      }
    }
  }
}
