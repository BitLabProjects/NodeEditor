using NodeEditor.Fbp;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class RemoveNodeCommandToken : ICommandToken {
    public readonly Graph Graph;
    public readonly Node Node;
    public RemoveNodeCommandToken(Graph graph, Node node) {
      this.Graph = graph;
      this.Node = node;
    }
  }

  class RemoveNodeCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public RemoveNodeCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as RemoveNodeCommandToken;
      
      mApp.SetGraph(mApp.Graph.RemoveNode(token.Node)); 
    }
  }
}
