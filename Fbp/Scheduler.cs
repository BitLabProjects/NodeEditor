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
        var component = Assembly.GetCallingAssembly().CreateInstance("NodeEditor.Fbp.Components." + node.Type + "Component", false) as Component;
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

        object[] inputs = new object[] { "Hello world!" };
        var result = process.Component.Run(inputs);
        if (!result.IsNull) {
          //TODO 
          //1. Find input(s) connected to result.Output
          //2. For each of them bind the input value
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
