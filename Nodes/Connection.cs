using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class Connection {
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

    public Connection ReplaceNode(Node oldN, Node newN) {
      var fromIsOld = (FromNode == oldN);
      var toIsOld = (ToNode == oldN);
      if (fromIsOld || toIsOld) {
        // TODO Verify also connections: the node is not the same so they must be checked
        return new Connection(fromIsOld ? newN : FromNode, FromNodeOutput,
                              toIsOld ? newN : ToNode, ToNodeInput);
      } else {
        return this;
      }
    }
  }
}
