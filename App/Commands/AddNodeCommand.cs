using NodeEditor.Fbp;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  class AddNodeCommandToken : ICommandToken {
    public readonly Graph Graph;
    public readonly string Type;
    public AddNodeCommandToken(Graph graph, string type) {
      this.Graph = graph;
      this.Type = type;
    }
  }

  class AddNodeCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public AddNodeCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as AddNodeCommandToken;

      // TODO Test if the connection already exists
      var pos = mApp.Graph.Nodes.Count == 0 ? new Point2(0, 0) : mApp.Graph.Nodes.Last().Position + new Point2(200, 0);
      var inputs = ImmutableArray<NodeInput>.Empty;
      var outputs = ImmutableArray<NodeOutput>.Empty;

      var type = ComponentFinder.FindByName(token.Type);
      foreach(ComponentInputAttribute attr in type.GetCustomAttributes(typeof(ComponentInputAttribute), true)) {
        inputs = inputs.Add(new NodeInput(attr.Name, null));
      }
      foreach (ComponentOutputAttribute attr in type.GetCustomAttributes(typeof(ComponentOutputAttribute), true)) {
        outputs = outputs.Add(new NodeOutput(attr.Name));
      }

      var node = new Node(token.Type, token.Type, pos,
                           inputs,
                           outputs);
      mApp.SetGraph(mApp.Graph.AddNode(node)); 
    }
  }
}
