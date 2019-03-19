using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class AddConnectionCommandToken : ICommandToken {
    public readonly Graph Graph;
    public readonly Connection Connection;
    public AddConnectionCommandToken(Graph graph, Connection connection) {
      this.Graph = graph;
      this.Connection = connection;
    }
  }

  class AddConnectionCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public AddConnectionCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as AddConnectionCommandToken;

      // TODO Test if the connection already exists
      mApp.SetGraph(mApp.Graph.AddConnection(token.Connection)); 
    }
  }
}
