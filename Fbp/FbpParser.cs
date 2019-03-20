using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  public class FbpParseResult {
    public readonly ImmutableList<Component> Components;
    public FbpParseResult(ImmutableList<Component> components) {
      this.Components = components;
    }
  }
  public class Component {
    public readonly string Name;
    public readonly string Type;
    public ImmutableList<string> InputPorts;
    public ImmutableList<string> OutputPorts;
    public ImmutableList<Tuple<string, Component, string>> OutputPortConnections;
    public Component(string name, string type) {
      this.Name = name;
      this.Type = type;
      InputPorts = ImmutableList<string>.Empty;
      OutputPorts = ImmutableList<string>.Empty;
      OutputPortConnections = ImmutableList<Tuple<string, Component, string>>.Empty;
    }
    internal void ConnectTo(string senderPort, Component receiver, string receiverPort) {
      senderPort = senderPort.Substring(0, 1).ToUpperInvariant() + senderPort.Substring(1).ToLowerInvariant();
      receiverPort = receiverPort.Substring(0, 1).ToUpperInvariant() + receiverPort.Substring(1).ToLowerInvariant();

      if (!OutputPorts.Contains(senderPort)) {
        OutputPorts = OutputPorts.Add(senderPort);
      }
      if (!receiver.InputPorts.Contains(receiverPort)) {
        receiver.InputPorts = receiver.InputPorts.Add(receiverPort);
      }
      OutputPortConnections = OutputPortConnections.Add(Tuple.Create(senderPort, receiver, receiverPort));
    }

    internal void SetInitialData(string receiverPort, object v) {
      //throw new NotImplementedException();
    }

    internal void Setup() {
      //throw new NotImplementedException();
    }
  }

  public static class FbpParser {
    const string COMMENT = @"^#.*$";
    const string COMPONENT_DECLARATION = @"^[A-Za-z]+(\w|\(|\)|_)*\.[A-Za-z]+(\w|\(|\)|_)*$";
    const string COMPONENT_WITH_TYPE_DECLARATION = @"^(\w+)\(([a-zA-Z]+(/[a-zA-Z]+)?)\)$";
    const string PORT_AND_COMPONENT = @"^(\w+) (\w+)$";
    const string PORT_AND_COMPONENT_WITH_TYPE = @"^(\w+) (\w+\([a-zA-Z]+(/[a-zA-Z]+)?\))$";
    const string COMPONENT_AND_PORT = @"^(\w+) (\w+)$";
    const string COMPONENT_AND_PORT_WITH_TYPE = @"^(\w+\([a-zA-Z]+(/[a-zA-Z]+)?\)) (\w+)$";
    const string INITIAL_BOOL_DATA = @"^(true|false)$";
    const string INITIAL_STRING_DATA = @"^('|"")(.+)\1$";
    const string INITIAL_FLOAT_DATA = @"^\d+\.\d+$";
    const string INITIAL_INT_DATA = @"^\d+$";
    const string INITIAL_PASSED_IN_DATA = @"^\<(.+)\>$";


    // https://noflojs.org/documentation/graphs/

    public static FbpParseResult Parse(string fbpProgram) {
      return Parse(fbpProgram, null);
    }

    private static FbpParseResult Parse(string fbpProgram, Dictionary<string, object> initialData) {
      var components = new Dictionary<string, Component>();
      var lines = fbpProgram.Split('\n');
      if (initialData == null) {
        initialData = new Dictionary<string, object>();
      }

      for (var i = 0; i < lines.Length; ++i) {
        var line = lines[i].Trim();
        if (Regex.IsMatch(line, COMMENT)) {
          continue;
        }

        var separatorIndex = line.IndexOf("->");
        if (separatorIndex < 0) {
          continue;
        }

        var senderText = line.Substring(0, separatorIndex).Trim();
        var receiverText = line.Substring(separatorIndex + 2).Trim();

        //var receiverParts = receiverText.Split('.');
        //if (receiverParts.Length != 2) {
        //  throw new ArgumentException(string.Format("Invalid input on line {0}, receiver declaration is malformed: '{1}'", i, receiverText));
        //}
        //var receiver = CreateOrRetrieveComponentFromString(receiverParts[0].Trim(), i, scheduler);
        //var receiverPort = receiverParts[1].Trim();

        Component receiver = null;
        string receiverPort = null;
        //receiverText examples:
        //IN HoldMode
        //STRING HoldMode(strings/SendString)
        if (Regex.IsMatch(receiverText, PORT_AND_COMPONENT)) {
          var match = Regex.Match(receiverText, PORT_AND_COMPONENT);
          receiverPort = match.Groups[1].Value;
          receiver = CreateOrRetrieveComponentFromString(match.Groups[2].Value, i, components);
        } else if (Regex.IsMatch(receiverText, PORT_AND_COMPONENT_WITH_TYPE)) {
          var match = Regex.Match(receiverText, PORT_AND_COMPONENT_WITH_TYPE);
          receiverPort = match.Groups[1].Value;
          receiver = CreateOrRetrieveComponentFromString(match.Groups[2].Value, i, components);
        } else {
          throw new ArgumentException(string.Format("Invalid input on line {0}, receiver declaration is malformed: '{1}'", i, receiverText));
        }


        if (Regex.IsMatch(senderText, COMPONENT_AND_PORT)) {
          var match = Regex.Match(senderText, COMPONENT_AND_PORT);
          var sender = CreateOrRetrieveComponentFromString(match.Groups[1].Value, i, components);
          var senderPort = match.Groups[2].Value;
          sender.ConnectTo(senderPort, receiver, receiverPort);

        } else if (Regex.IsMatch(senderText, COMPONENT_AND_PORT_WITH_TYPE)) {
          var match = Regex.Match(senderText, COMPONENT_AND_PORT_WITH_TYPE);
          var sender = CreateOrRetrieveComponentFromString(match.Groups[1].Value, i, components);
          var senderPort = match.Groups[3].Value;
          sender.ConnectTo(senderPort, receiver, receiverPort);

        } else if (Regex.IsMatch(senderText, INITIAL_PASSED_IN_DATA)) {
          var match = Regex.Match(senderText, INITIAL_PASSED_IN_DATA);
          var key = match.Groups[1].Value;
          if (initialData.ContainsKey(key)) {
            receiver.SetInitialData(receiverPort, initialData[key]);
          } else {
            throw new ArgumentException(string.Format("Initial data {0} was not found in the provided Dictionary", key));
          }

        } else if (Regex.IsMatch(senderText, INITIAL_BOOL_DATA)) {
          var match = Regex.Match(senderText, INITIAL_BOOL_DATA);
          receiver.SetInitialData(receiverPort, bool.Parse(match.Value));

        } else if (Regex.IsMatch(senderText, INITIAL_STRING_DATA)) {
          var match = Regex.Match(senderText, INITIAL_STRING_DATA);
          receiver.SetInitialData(receiverPort, match.Groups[2].Value);

        } else if (Regex.IsMatch(senderText, INITIAL_FLOAT_DATA)) {
          var match = Regex.Match(senderText, INITIAL_FLOAT_DATA);
          receiver.SetInitialData(receiverPort, float.Parse(match.Value));

        } else if (Regex.IsMatch(senderText, INITIAL_INT_DATA)) {
          var match = Regex.Match(senderText, INITIAL_INT_DATA);
          receiver.SetInitialData(receiverPort, int.Parse(match.Value));

        } else {
          throw new ArgumentException(string.Format("Invalid input on line {0}, sender declaration is malformed: '{1}'", i, senderText));
        }
      }

      return new FbpParseResult(ImmutableList<Component>.Empty.AddRange(components.Values));
    }

    static Component CreateOrRetrieveComponentFromString(string stringVersionOfComponent, 
                                                         int sourceLineNumber, 
                                                         Dictionary<string, Component> components) {
      string componentName;
      string typeName = null;

      if (Regex.IsMatch(stringVersionOfComponent, COMPONENT_WITH_TYPE_DECLARATION)) {
        var match = Regex.Match(stringVersionOfComponent, COMPONENT_WITH_TYPE_DECLARATION);
        componentName = match.Groups[1].Value;
        typeName = match.Groups[2].Value;
      } else {
        componentName = stringVersionOfComponent;
      }

      if (typeName != null) {
        if (components.ContainsKey(componentName)) {
          throw new ArgumentException(string.Format("Invalid input on line {0}, process '{1}' has already been declared", sourceLineNumber, componentName));
        } else {
          /*
          Type resolvedType = null;
          var assemblies = AppDomain.CurrentDomain.GetAssemblies();
          foreach (var assembly in assemblies) {
            var types = assembly.GetTypes();
            foreach (var type in types) {
              if (type.Name.EndsWith(typeName)) {
                resolvedType = type;
              }
            }
          }

          if (resolvedType == null) {
            throw new ArgumentException(string.Format("Invalid input on line {0}, the type '{1}' declared for process '{2}' could not be found", sourceLineNumber, typeName, componentName));
          }
          if (!typeof(Component).IsAssignableFrom(resolvedType)) {
            throw new ArgumentException(string.Format("Invalid input on line {0}, the type '{1}' declared for process '{2}' is not a component", sourceLineNumber, typeName, componentName));
          }

          var component = (Component)System.Activator.CreateInstance(resolvedType, new[] { componentName });
          */
          var component = new Component(componentName, typeName);
          component.Setup();
          components[componentName] = component;
        }
      }

      if (!components.ContainsKey(componentName)) {
        throw new ArgumentException(string.Format("Invalid input on line {0}, the process '{1}' has not been declared", sourceLineNumber, componentName));
      }
      return components[componentName];
    }
  }
}
