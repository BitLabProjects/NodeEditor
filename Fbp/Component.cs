using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  public abstract class Component {
    public abstract RunOutput Run(object[] inputs);

    public struct RunOutput {
      public readonly int OutputIdx;
      public readonly object Value;
      public RunOutput(int outputIdx, object value) {
        this.OutputIdx = outputIdx;
        this.Value = value;
      }

      public bool IsNull => OutputIdx == -1;

      public static RunOutput Null { get { return new RunOutput(-1, null); } }
    }
  }
}
