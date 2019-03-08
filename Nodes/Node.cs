﻿using NodeEditor.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  class Node {
    public Point2 Position { get; }
    public ImmutableList<NodeInput> Inputs { get; }
    public ImmutableList<NodeOutput> Outputs { get; }

    public Node(Point2 position, ImmutableList<NodeInput> inputs, ImmutableList<NodeOutput> outputs) {
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
  }
}