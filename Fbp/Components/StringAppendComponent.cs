using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  [ComponentInput(0, "String1", "string")]
  [ComponentInput(1, "String2", "string")]
  [ComponentOutput(0, "Result", "string")]
  public class StringAppendComponent : Component {
    public override RunOutput Run(object[] inputs) {
      return new RunOutput(0, (inputs[0] as string) + (inputs[1] as string));
    }
  }
}
