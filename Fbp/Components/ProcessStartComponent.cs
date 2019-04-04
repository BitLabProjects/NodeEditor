using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  [ComponentInput(0, "Path", "string")]
  [ComponentInput(1, "Arguments", "string")]
  public class ProcessStartComponent: Component {
    public override RunOutput Run(object[] inputs) {
      var process = new System.Diagnostics.Process();
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName = inputs[0] as string;
      process.StartInfo.Arguments = inputs[1] as string;
      process.Start();
      process.WaitForExit();
      
      return RunOutput.Null;
    }
  }
}
