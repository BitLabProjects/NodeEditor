using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class NodePort {
    public string Name { get; }

    public NodePort(string name) {
      this.Name = name;
    }
  }
}
