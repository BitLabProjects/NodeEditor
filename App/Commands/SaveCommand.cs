using NodeEditor.Fbp;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeEditor.App.Commands {
  class SaveCommandToken : ICommandToken {
    public SaveCommandToken() {
    }
  }

  class SaveCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public SaveCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as SaveCommandToken;

      FbpWriter.Write(mApp.Graph, mApp.CurrentFbpFullFileName);
    }
  }
}
