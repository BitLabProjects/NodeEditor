using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class EditNodeInputInitialDataToken : ICommandToken {
    public readonly Graph Graph;
    public readonly Node Node;
    public readonly NodeInput NodeInput;
    public readonly object NewInitialData;
    public EditNodeInputInitialDataToken(Graph graph, Node node, NodeInput nodeInput, object newInitialData) {
      this.Graph = graph;
      this.Node = node;
      this.NodeInput = nodeInput;
      this.NewInitialData = newInitialData;
    }
  }

  class EditNodeInputInitialDataCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public EditNodeInputInitialDataCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as EditNodeInputInitialDataToken;

      // TODO Test if the connection already exists
      var newNodeInput = new NodeInput(token.NodeInput.Name, token.NewInitialData);
      mApp.SetGraph(mApp.Graph.ReplaceNode(token.Node, token.Node.ReplaceInput(token.NodeInput, newNodeInput))); 
    }
  }
}
