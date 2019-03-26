using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  [ComponentInput(0, "Text", "string")]
  public class ConsoleWriteComponent : Component {
    public override RunOutput Run(object[] inputs) {
      var asArray = inputs[0] as Array;
      if (asArray != null) {
        Console.Write("[");
        for(var i=0; i< asArray.Length; i++) {
          if (i != 0)
            Console.Write(", ");
          Console.Write(asArray.GetValue(i).ToString());
        }
        Console.WriteLine("]");
      } else {
        Console.WriteLine(inputs[0]);
      }
      return RunOutput.Null;
    }
  }
}
