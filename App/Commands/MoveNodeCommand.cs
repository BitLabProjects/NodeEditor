using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class MoveNodeCommandToken : ICommandToken {
    public readonly Graph Graph;
    public readonly Node Node;
    public readonly Point2 NewPosition;
    public MoveNodeCommandToken(Graph graph, Node node, Point2 newPosition) {
      this.Graph = graph;
      this.Node = node;
      this.NewPosition = newPosition;
    }
  }

  class MoveNodeCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public MoveNodeCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as MoveNodeCommandToken;

      // TODO Test if the connection already exists
      mApp.SetGraph(mApp.Graph.ReplaceNode(token.Node, token.Node.Move(token.NewPosition))); 
    }
  }
}
