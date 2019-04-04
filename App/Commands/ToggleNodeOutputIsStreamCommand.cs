using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class ToggleNodeOutputIsStreamCommandToken : ICommandToken {
    public readonly Graph Graph;
    public readonly Node Node;
    public readonly NodeOutput NodeOutput;
    public readonly object NewInitialData;
    public ToggleNodeOutputIsStreamCommandToken(Graph graph, Node node, NodeOutput nodeOutput) {
      this.Graph = graph;
      this.Node = node;
      this.NodeOutput = nodeOutput;
    }
  }

  class ToggleNodeOutputIsStreamCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public ToggleNodeOutputIsStreamCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as ToggleNodeOutputIsStreamCommandToken;

      // TODO Test if the connection already exists
      var newNodeOutput = new NodeOutput(token.NodeOutput.Name, !token.NodeOutput.IsStream);
      mApp.SetGraph(mApp.Graph.ReplaceNode(token.Node, token.Node.ReplaceOutput(token.NodeOutput, newNodeOutput))); 
    }
  }
}
