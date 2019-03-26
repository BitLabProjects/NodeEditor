using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  public class Scheduler {
    private readonly Graph graph;
    private ImmutableList<Process> runnableProcesses;
    private ImmutableDictionary<Node, Process> nodeToProcess;

    public Scheduler(Graph graph) {
      this.graph = graph;
      runnableProcesses = ImmutableList<Process>.Empty;
      nodeToProcess = ImmutableDictionary<Node, Process>.Empty;
    }

    private void InstantiateProcesses() {
      foreach(var node in graph.Nodes) {
        //var component = Assembly.GetCallingAssembly().CreateInstance("NodeEditor.Fbp.Components." + node.Type + "Component", false) as Component;
        var component = Activator.CreateInstance(ComponentFinder.FindByName(node.Type)) as Component;
        if (component == null) {
          throw new ArgumentException($"Could not create process for component '{node.Type}'");
        }
        var process = new Process(node, component);
        nodeToProcess = nodeToProcess.Add(node, process);

        var isRunnable = false;
        for (var i = 0; i < node.Inputs.Length; i++) {
          if (node.Inputs[i].InitialData != null) {
            isRunnable = isRunnable | process.SetInputValue(i, node.Inputs[i].InitialData);
          }
        }
        if (isRunnable) {
          runnableProcesses = runnableProcesses.Add(process);
        }
      }
    }

    public void Run() {
      InstantiateProcesses();

      while (runnableProcesses.Count > 0) {
        var process = runnableProcesses[0];
        runnableProcesses = runnableProcesses.RemoveAt(0);

        var result = process.Run();
        if (!result.IsNull) {

          var nodeOutput = process.Node.Outputs[result.OutputIdx];

          //1. Find input(s) connected to result.Output
          var connections = graph.Connections.Where((c) => c.FromNode == process.Node && c.FromNodeOutput == nodeOutput).ToList();

          //2. For each of them bind the input value
          foreach(var c in connections) {
            var toProcess = nodeToProcess[c.ToNode];
            var inputIdx = c.ToNode.GetInputIndex(c.ToNodeInput);
            var toProcessHasBecomeRunnable = toProcess.SetInputValue(inputIdx, result.Value);
            if (toProcessHasBecomeRunnable) {
              runnableProcesses = runnableProcesses.Add(toProcess);
            }
          }

          //TODO 
          //3. If the node has all its input filled, move it to runnableProcesses
        }
      }
    }
  }


  public class Process {
    public readonly Node Node;
    public readonly Component Component;
    private object[] InputValues;
    public Process(Node node, Component component) {
      this.Node = node;
      this.Component = component;
      InputValues = new object[node.Inputs.Length];
    }

    public Component.RunOutput Run() {
      return Component.Run(InputValues);
    }

    public bool SetInputValue(int inputIdx, object value) {
      InputValues[inputIdx] = value;
      for(var i=0; i<InputValues.Length; i++) {
        if (InputValues[i] == null) {
          return false;
        }
      }
      return true;
    }
  }
}
