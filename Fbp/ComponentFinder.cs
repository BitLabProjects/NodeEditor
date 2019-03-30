using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  public class ComponentFinder {
    public static ImmutableArray<string> GetAllComponents() {
      var result = ImmutableArray<string>.Empty;
      foreach (var t in Assembly.GetCallingAssembly().ExportedTypes) {
        if (t.IsSubclassOf(typeof(Component))) {
          result = result.Add(t.Name.Replace("Component", ""));
        }
      }
      return result;
    }

    public static Type FindByName(string componentName) {
      foreach (var t in Assembly.GetCallingAssembly().ExportedTypes) {
        if (t.IsSubclassOf(typeof(Component)) && t.Name == componentName + "Component") {
          return t;
        }
      }
      return null;
    }

    public static ImmutableArray<ComponentInputAttribute> GetInputAttributes(Type componentType) {
      var attributes = componentType.GetCustomAttributes(typeof(ComponentInputAttribute), true).Cast<ComponentInputAttribute>();
      return ImmutableArray<ComponentInputAttribute>.Empty.AddRange(attributes);
    }

    public static ImmutableArray<ComponentOutputAttribute> GetOutputAttributes(Type componentType) {
      var attributes = componentType.GetCustomAttributes(typeof(ComponentOutputAttribute), true).Cast<ComponentOutputAttribute>();
      return ImmutableArray<ComponentOutputAttribute>.Empty.AddRange(attributes);
    }
  }
}
