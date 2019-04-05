using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  [ComponentInput(0, "Format", "string")]
  [ComponentInput(1, "String1", "string")]
  [ComponentOutput(0, "Result", "string")]
  public class StringFormatComponent : Component {
    public override RunOutput Run(object[] inputs) {
      var result = String.Format(inputs[0] as string, inputs[1] as string);
      return new RunOutput(0, result);
    }
  }
}
