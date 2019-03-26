using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class NodeInput: NodePort {
    public object InitialData { get; }
    public NodeInput(string name, object initialData): base(name) {
      this.InitialData = initialData;
    }
  }
}
