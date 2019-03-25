using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  public class ConsoleWriteComponent : Component {
    public override RunOutput Run(object[] inputs) {
      Console.WriteLine(inputs[0]);
      return RunOutput.Null;
    }
  }
}
