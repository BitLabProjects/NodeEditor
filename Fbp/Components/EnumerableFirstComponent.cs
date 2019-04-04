using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp.Components {
  [ComponentInput(0, "Enumerable", "IEnumerable<Object>")]
  [ComponentOutput(0, "First", "Object")]
  public class EnumerableFirstComponent : Component {
    public override RunOutput Run(object[] inputs) {
      return new RunOutput(0, (inputs[0] as IEnumerable<object>).First());
    }
  }
}
