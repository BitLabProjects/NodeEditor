using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class NodeOutput : NodePort {
    public bool IsStream { get; }

    public NodeOutput(string name, bool isStream) : base(name) {
      this.IsStream = isStream;
    }
  }
}
