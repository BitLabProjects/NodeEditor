using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  internal struct ProcessOutputStream {
    public readonly Process Process;
    public readonly int OutputIdx;
    public readonly IEnumerator<object> ValueEnumerator;
    public ProcessOutputStream(Process Process, int OutputIdx, IEnumerator<object> ValueEnumerator) {
      this.Process = Process;
      this.OutputIdx = OutputIdx;
      this.ValueEnumerator = ValueEnumerator;
    }
  }

  public class Scheduler {
    private readonly Graph graph;
    private ImmutableDictionary<Node, Process> nodeToProcess;
    private ImmutableQueue<Process> runnableProcesses;
    // Output streaming
    private ImmutableList<ProcessOutputStream> runningOutputStreams;

    public Scheduler(Graph graph) {
      this.graph = graph;
      nodeToProcess = ImmutableDictionary<Node, Process>.Empty;
      runnableProcesses = ImmutableQueue<Process>.Empty;
      runningOutputStreams = ImmutableList<ProcessOutputStream>.Empty;
    }

    public void Run() {
      InstantiateProcesses();

      while (!runnableProcesses.IsEmpty) {
        Process process;
        runnableProcesses = runnableProcesses.Dequeue(out process);

        var result = process.Run();
        if (!result.IsNull) {

          var nodeOutput = process.Node.Outputs[result.OutputIdx];
          if (nodeOutput.IsStream) {
            if (GetNodeOutputIsCurrentlyStreaming(process, result.OutputIdx)) {
              // Error!
              throw new ArgumentException($"Output is already streaming");
            }

            IEnumerator<object> enumerator = ((IEnumerable<object>)result.Value).GetEnumerator();
            Run_PropagateOutputStream(new ProcessOutputStream(process, result.OutputIdx, enumerator));
          } else {
            Run_PropagateOutputValue(process, result.OutputIdx, result.Value);
          }
        }

        // This process has executed: its inputs are all empty now, so an incoming stream could be triggered again
        Run_MaybePropagateOutputStreamsAfterProcessHasRun();
      }
    }

    private void Run_PropagateOutputStream(ProcessOutputStream outputStream) {
      if (!outputStream.ValueEnumerator.MoveNext()) {
        // Stream is finished
        return;
      }

      Run_PropagateOutputValue(outputStream.Process, outputStream.OutputIdx, outputStream.ValueEnumerator.Current);

      // Enqueue again for the next stream value
      runningOutputStreams = runningOutputStreams.Add(outputStream);
    }

    private void Run_PropagateOutputValue(Process process, int outputIdx, object value) {
      var nodeOutput = process.Node.Outputs[outputIdx];

      //1. Find input(s) connected to result.Output
      var connections = graph.GetAllConnectionsForNodeOutput(process.Node, nodeOutput);

      //2. For each of them bind the input value
      foreach (var c in connections) {
        var toProcess = nodeToProcess[c.ToNode];
        var inputIdx = c.ToNode.GetInputIndex(c.ToNodeInput);
        var toProcessHasBecomeRunnable = toProcess.SetInputValue(inputIdx, value);
        if (toProcessHasBecomeRunnable) {
          runnableProcesses = runnableProcesses.Enqueue(toProcess);
        }
      }
    }

    private void Run_MaybePropagateOutputStreamsAfterProcessHasRun() {
      // If any output stream currently running is connected to an input of ranProcess, and all its other inputs are empty, propagate it
      bool somethingDone = true;
      while (somethingDone) {
        somethingDone = false;
        for (var i = 0; i < runningOutputStreams.Count; i++) {
          var outputStream = runningOutputStreams[i];
          // This stream can run if all the inputs connected to the output are empty
          var nodeOutput = outputStream.Process.Node.Outputs[outputStream.OutputIdx];
          var connections = graph.GetAllConnectionsForNodeOutput(outputStream.Process.Node, nodeOutput);

          bool allInputsAreEmpty = true;
          foreach (var c in connections) {
            var toProcess = nodeToProcess[c.ToNode];
            var inputIdx = c.ToNode.GetInputIndex(c.ToNodeInput);
            if (toProcess.InputValueIsSet(inputIdx)) {
              allInputsAreEmpty = false;
              break;
            }
          }

          if (allInputsAreEmpty) {
            runningOutputStreams = runningOutputStreams.RemoveAt(i);
            Run_PropagateOutputStream(outputStream);
            somethingDone = true;
            break;
          }
        }
      }
    }

    private bool GetNodeOutputIsCurrentlyStreaming(Process process, int outputIdx) {
      return runningOutputStreams.Any((x) => x.Process == process && x.OutputIdx == outputIdx);
    }

    private void InstantiateProcesses() {
      foreach (var node in graph.Nodes) {
        var component = Activator.CreateInstance(ComponentFinder.FindByName(node.Type)) as Component;
        if (component == null) {
          throw new ArgumentException($"Could not create process for component '{node.Type}'");
        }
        var process = new Process(node, component);
        nodeToProcess = nodeToProcess.Add(node, process);

        if (process.FillInitialData()) {
          runnableProcesses = runnableProcesses.Enqueue(process);
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

    public bool FillInitialData() {
      var isRunnable = false;
      for (var i = 0; i < Node.Inputs.Length; i++) {
        if (Node.Inputs[i].InitialData != null) {
          isRunnable = isRunnable | SetInputValue(i, Node.Inputs[i].InitialData);
        }
      }
      return isRunnable;
    }

    public Component.RunOutput Run() {
      var localIV = InputValues;

      InputValues = new object[InputValues.Length];
      if (FillInitialData()) {
        // What do we do with this information? It probably is ok to do nothing: we don't want a process with only inputs with initial datas to trigger continuously
      }

      return Component.Run(localIV);
    }

    public bool InputValueIsSet(int inputIdx) {
      return InputValues[inputIdx] != null;
    }

    public bool SetInputValue(int inputIdx, object value) {
      InputValues[inputIdx] = value;
      for (var i = 0; i < InputValues.Length; i++) {
        if (InputValues[i] == null) {
          return false;
        }
      }
      return true;
    }
  }
}
