using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class NodeInput: NodePort {
    public readonly object InitialData;
    public NodeInput(string name, object initialData): base(name) {
      this.InitialData = initialData;
    }
  }
}
