using NodeEditor.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class Node {
    public string Name { get; }
    public string Type { get; }
    public Point2 Position { get; }
    public ImmutableArray<NodeInput> Inputs { get; }
    public ImmutableArray<NodeOutput> Outputs { get; }

    public Node(string name,
                string type, 
                Point2 position, 
                ImmutableArray<NodeInput> inputs, 
                ImmutableArray<NodeOutput> outputs) {
      this.Name = name;
      this.Type = type;
      this.Position = position;
      this.Inputs = inputs;
      this.Outputs = outputs;
    }

    public Int32 GetInputIndex(NodeInput input) {
      return Inputs.IndexOf(input);
    }
    public Int32 GetOutputIndex(NodeOutput output) {
      return Outputs.IndexOf(output);
    }

    #region Manipulation
    public Node Move(Point2 newPosition) {
      return new Node(Name, Type, newPosition, Inputs, Outputs);
    }
    public Node ReplaceInput(NodeInput oldNI, NodeInput newNI) {
      var newInputs = Inputs.Replace(oldNI, newNI);
      return new Node(Name, Type, Position, newInputs, Outputs);
    }
    public Node ReplaceOutput(NodeOutput oldNO, NodeOutput newNO) {
      var newOutputs = Outputs.Replace(oldNO, newNO);
      return new Node(Name, Type, Position, Inputs, newOutputs);
    }
    #endregion
  }
}
