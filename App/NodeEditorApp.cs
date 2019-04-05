using NodeEditor.App.Commands;
using NodeEditor.Fbp;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

namespace NodeEditor.App {
  class NodeEditorApp : INotifyPropertyChanged {
    public Graph Graph { get; private set; }
    public string CurrentFbpFullFileName { get; private set; }
    private ImmutableList<Graph> mHistory;
    private int mCurrentHistoryIndex;
    public CommandManager CommandManager { get; }

    //
    public ImmutableArray<string> LoadedComponents { get; }

    public NodeEditorApp() {
      mHistory = ImmutableList<Graph>.Empty;
      mCurrentHistoryIndex = -1;

      CommandManager = new CommandManager();
      CommandManager.RegisterCommand(typeof(RemoveConnectionCommandToken), () => new RemoveConnectionCommand(this));
      CommandManager.RegisterCommand(typeof(AddConnectionCommandToken), () => new AddConnectionCommand(this));
      CommandManager.RegisterCommand(typeof(MoveNodeCommandToken), () => new MoveNodeCommand(this));
      CommandManager.RegisterCommand(typeof(UndoRedoCommandToken), () => new UndoRedoCommand(this));
      CommandManager.RegisterCommand(typeof(PlayCommandToken), () => new PlayCommand(this));
      CommandManager.RegisterCommand(typeof(EditNodeInputInitialDataToken), () => new EditNodeInputInitialDataCommand(this));
      CommandManager.RegisterCommand(typeof(AddNodeCommandToken), () => new AddNodeCommand(this));
      CommandManager.RegisterCommand(typeof(RemoveNodeCommandToken), () => new RemoveNodeCommand(this));
      CommandManager.RegisterCommand(typeof(SaveCommandToken), () => new SaveCommand(this));
      CommandManager.RegisterCommand(typeof(ToggleNodeOutputIsStreamCommandToken), () => new ToggleNodeOutputIsStreamCommand(this));

      LoadedComponents = ComponentFinder.GetAllComponents();

      CurrentFbpFullFileName = @"..\..\TestData\FbpGraphs\HelloWorld.fbp";
      SetGraph(FbpReader.Read(CurrentFbpFullFileName));
    }

    private void mLoadTestGraph() {
      var node1 = new Node("Node 1", "type", new Point2(250, -80),
                           ImmutableArray<NodeInput>.Empty,
                           ImmutableArray<NodeOutput>.Empty);
      var node2 = new Node("Node 2", "type", new Point2(200, 150),
                           ImmutableArray<NodeInput>.Empty.Add(new NodeInput("Input 1", null)).Add(new NodeInput("Input 2", null)),
                           ImmutableArray<NodeOutput>.Empty);
      var node3 = new Node("Node 3", "type", new Point2(-100, -80),
                           ImmutableArray<NodeInput>.Empty.Add(new NodeInput("Input 1", null)).Add(new NodeInput("Input 2", null)),
                           ImmutableArray<NodeOutput>.Empty.Add(new NodeOutput("Output 1", false)).Add(new NodeOutput("Output 2", false)));
      var node4 = new Node("Node 4", "type", new Point2(-50, 80),
                           ImmutableArray<NodeInput>.Empty,
                           ImmutableArray<NodeOutput>.Empty.Add(new NodeOutput("Output 1", false)).Add(new NodeOutput("Output 2", false)));
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
      SetGraph(new Graph(nodes, connections));
    }

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    internal void SetGraph(Graph newGraph) {
      // Discard entries beyond current position
      if (mCurrentHistoryIndex >= 0) {
        mHistory = mHistory.GetRange(0, mCurrentHistoryIndex + 1);
      }
      // And append the new one
      mHistory = mHistory.Add(newGraph);

      mCurrentHistoryIndex = mHistory.Count - 1;
      SetCurrentGraphAndNotify();
    }
    internal void Undo() {
      if (mHistory.Count == 0) {
        // Initial state
        return;
      }
      if (mCurrentHistoryIndex > 0) {
        mCurrentHistoryIndex -= 1;
        SetCurrentGraphAndNotify();
      }
    }
    internal void Redo() {
      if (mHistory.Count == 0) {
        // Initial state
        return;
      }
      if (mCurrentHistoryIndex < mHistory.Count - 1) {
        mCurrentHistoryIndex += 1;
        SetCurrentGraphAndNotify();
      }
    }
    private void SetCurrentGraphAndNotify() {
      System.Diagnostics.Debug.WriteLine("Setting graph #" + mCurrentHistoryIndex);
      Graph = mHistory[mCurrentHistoryIndex];
      Notify(nameof(Graph));
    }

    private void Notify(string propName) {
      if (PropertyChanged != null) {
        PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
      }
    }
  }
}
