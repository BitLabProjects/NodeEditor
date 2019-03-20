using NodeEditor.App.Commands;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App {
  class NodeEditorApp: INotifyPropertyChanged {
    public Graph Graph { get; private set; }
    public CommandManager CommandManager { get; }
    public NodeEditorApp() {
      var node1 = new Node("Node 1", new Point2(250, -80),
                           ImmutableList<NodeInput>.Empty,
                           ImmutableList<NodeOutput>.Empty);
      var node2 = new Node("Node 2", new Point2(200, 150),
                           ImmutableList<NodeInput>.Empty.Add(new NodeInput("Input 1")).Add(new NodeInput("Input 2")),
                           ImmutableList<NodeOutput>.Empty);
      var node3 = new Node("Node 3", new Point2(-100, -80),
                           ImmutableList<NodeInput>.Empty.Add(new NodeInput("Input 1")).Add(new NodeInput("Input 2")),
                           ImmutableList<NodeOutput>.Empty.Add(new NodeOutput("Output 1")).Add(new NodeOutput("Output 2")));
      var node4 = new Node("Node 4", new Point2(-50, 80),
                           ImmutableList<NodeInput>.Empty,
                           ImmutableList<NodeOutput>.Empty.Add(new NodeOutput("Output 1")).Add(new NodeOutput("Output 2")));
      var nodes = ImmutableList<Node>
        .Empty
        .Add(node1)
        .Add(node2)
        .Add(node3)
        .Add(node4);

      //for(var i=0; i<100; i++) {
      //  var nodeI = new Node(new Point2(i*10, 300),
      //                       ImmutableList<NodeInput>.Empty,
      //                       ImmutableList<NodeOutput>.Empty);
      //  nodes = nodes.Add(nodeI);
      //}

      var connections = ImmutableList<Connection>
        .Empty
        .Add(new Connection(node3, node3.Outputs[0], node2, node2.Inputs[1]))
        .Add(new Connection(node4, node4.Outputs[1], node2, node2.Inputs[0]));
      Graph = new Graph(nodes, connections);

      Graph = mParseFbpFile(@"C:\Dati\Programmazione\Progetti\NodeEditor\TestData\FbpGraphs\Delete.fbp");

      CommandManager = new CommandManager();
      CommandManager.RegisterCommand(typeof(RemoveConnectionCommandToken), () => new RemoveConnectionCommand(this));
      CommandManager.RegisterCommand(typeof(AddConnectionCommandToken), () => new AddConnectionCommand(this));
    }

    private Graph mParseFbpFile(string fbpFullFileName) {
      string fbpContent = System.IO.File.ReadAllText(fbpFullFileName);
      var result = Fbp.FbpParser.Parse(fbpContent);

      var nodes = ImmutableList<Node>.Empty;
      for (var i=0; i<result.Components.Count; i++) {
        var component = result.Components[i];
        var inputs = ImmutableList<NodeInput>.Empty.AddRange(from x in component.InputPorts
                                                             select new NodeInput(x));
        var outputs = ImmutableList<NodeOutput>.Empty.AddRange(from x in component.OutputPorts
                                                               select new NodeOutput(x));
        var node = new Node(component.Name,
                            new Point2(i * 200, i * 50),
                            inputs,
                            outputs);
        nodes = nodes.Add(node);
      }

      var connections = ImmutableList<Connection>.Empty;
      for (var i = 0; i < result.Components.Count; i++) {
        var component = result.Components[i];
        var fromNode = nodes.Where((x) => x.Name == component.Name).First();

        foreach (var conn in component.OutputPortConnections) {
          var fromNodeOutput = fromNode.Outputs.Where((x) => x.Name == conn.Item1).First();
          var toNode = nodes.Where((x) => x.Name == conn.Item2.Name).First();
          var toNodeInput = toNode.Inputs.Where((x) => x.Name == conn.Item3).First();
          connections = connections.Add(new Connection(fromNode, fromNodeOutput, toNode, toNodeInput));
        }
      }

      return new Graph(nodes, connections);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    internal void SetGraph(Graph newGraph) {
      Graph = newGraph;
      if (PropertyChanged != null) {
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Graph)));
      }
    }
  }
}
