using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  class Connection {
    public Node FromNode { get; }
    public NodeOutput FromNodeOutput { get; }
    public Node ToNode { get; }
    public NodeInput ToNodeInput { get; }

    public Connection(Node fromNode,
                      NodeOutput fromNodeOutput,
                      Node toNode,
                      NodeInput toNodeInput) {
      this.FromNode = fromNode;
      this.FromNodeOutput = fromNodeOutput;
      this.ToNode = toNode;
      this.ToNodeInput = toNodeInput;
    }
  }
}
