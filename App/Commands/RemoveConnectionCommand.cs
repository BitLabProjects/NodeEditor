using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class RemoveConnectionCommandToken: ICommandToken {
    public readonly Graph Graph;
    public readonly Connection Connection;
    public RemoveConnectionCommandToken(Graph graph, Connection connection) {
      this.Graph = graph;
      this.Connection = connection;
    }
  }

  class RemoveConnectionCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public RemoveConnectionCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as RemoveConnectionCommandToken;

      mApp.SetGraph(mApp.Graph.RemoveConnection(token.Connection)); 
    }
  }
}
