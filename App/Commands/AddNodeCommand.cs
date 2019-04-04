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
      foreach(var attr in ComponentFinder.GetInputAttributes(type)) {
        inputs = inputs.Add(new NodeInput(attr.Name, null));
      }
      foreach (var attr in ComponentFinder.GetOutputAttributes(type)) {
        outputs = outputs.Add(new NodeOutput(attr.Name, false));
      }

      var node = new Node(FindUniqueName(mApp.Graph, token.Type), token.Type, pos,
                          inputs, outputs);
      mApp.SetGraph(mApp.Graph.AddNode(node)); 
    }

    private string FindUniqueName(Graph currentGraph, string componentName) {
      var maxN = 0;
      var pattern = "^" + componentName + "([0-9]+)$";
      for (var i=0; i<currentGraph.Nodes.Count; i++) {
        var node = currentGraph.Nodes[i];

        var match = Regex.Match(node.Name, pattern, RegexOptions.IgnoreCase);
        if (match.Success) {
          maxN = Math.Max(maxN, Int32.Parse(match.Groups[1].Value));
        }
      }
      return $"{componentName}{maxN + 1}";
    }
  }
}
