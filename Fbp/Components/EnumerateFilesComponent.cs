using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  [ComponentInput(0, "Path", "string")]
  [ComponentOutput(0, "Files", "string[]")]
  public class EnumerateFilesComponent: Component {
    public override RunOutput Run(object[] inputs) {
      var result = System.IO.Directory.EnumerateFiles(inputs[0] as string);
      return new RunOutput(0, result.ToArray());
    }
  }
}
