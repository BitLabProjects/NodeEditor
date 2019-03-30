using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  class GraphTypeChecker {
    private ImmutableDictionary<string, Type> mResolvedTypes;

    public GraphTypeChecker() {
      mResolvedTypes = ImmutableDictionary<string, Type>.Empty;
    }

    public Type TryResolveComponentName(string componentName) {
      Type result;
      if (!mResolvedTypes.TryGetValue(componentName, out result)) {
        result = ComponentFinder.FindByName(componentName);
        mResolvedTypes = mResolvedTypes.Add(componentName, result);
      }
      return result;
    }
  }
}
