using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  public static class FbpReader {
    const string COMMENT = @"^#.*$";
    const string COMPONENT_WITH_TYPE_DECLARATION = @"^(\w+)\(([a-zA-Z]+(?:/[a-zA-Z]+)?)(?::([a-zA-Z]+=[a-zA-Z0-9\.]+)(?:,([a-zA-Z]+=[a-zA-Z0-9\.]+))+)\)$";
    const string PORT_AND_COMPONENT = @"^(\w+) (\w+)$";
    const string PORT_AND_COMPONENT_WITH_TYPE = @"^(\w+) (\w+\([a-zA-Z]+(/[a-zA-Z]+)?(:([a-zA-Z]+=[a-zA-Z0-9\.]+)(,([a-zA-Z]+=[a-zA-Z0-9\.]+))+)\))$";
    const string COMPONENT_AND_PORT = @"^(\w+) (\w+)$";
    const string COMPONENT_AND_PORT_WITH_TYPE = @"^(\w+\([a-zA-Z]+(?:/[a-zA-Z]+)?(?::(?:[a-zA-Z]+=[a-zA-Z0-9\.]+)(?:,(?:[a-zA-Z]+=[a-zA-Z0-9\.]+))+)\)) (\w+)$";
    const string COMPONENT_METADATA = @"^([a-zA-Z]+)=([a-zA-Z0-9\.]+)$";
    const string INITIAL_BOOL_DATA = @"^(true|false)$";
    const string INITIAL_STRING_DATA = @"^('|"")(.+)\1$";
    const string INITIAL_FLOAT_DATA = @"^\d+\.\d+$";
    const string INITIAL_INT_DATA = @"^\d+$";
    const string INITIAL_PASSED_IN_DATA = @"^\<(.+)\>$";


    // https://noflojs.org/documentation/graphs/

    public static Graph Read(string fbpFullFileName) {
      string fbpContent = System.IO.File.ReadAllText(fbpFullFileName);
      var result = Fbp.FbpReader.Parse(fbpContent);

      var nodes = ImmutableList<Node>.Empty;
      for (var i = 0; i < result.Components.Count; i++) {
        var component = result.Components[i];
        var inputs = ImmutableArray<NodeInput>.Empty.AddRange(from x in component.InputPorts
                                                              select new NodeInput(x, component.InputPortInitialDatas.GetValueOrDefault(x, null)));
        var outputs = ImmutableArray<NodeOutput>.Empty.AddRange(from x in component.OutputPorts
                                                                select new NodeOutput(x));

        var pos = new Point2(Double.Parse(component.Metadata.GetValueOrDefault("x", "0")),
                             Double.Parse(component.Metadata.GetValueOrDefault("y", "0")));

        var node = new Node(component.Name, component.Type,
                            pos, inputs, outputs);
        nodes = nodes.Add(node);
      }

      var connections = ImmutableList<Connection>.Empty;
      for (var i = 0; i < result.Components.Count; i++) {
        var component = result.Components[i];
        var fromNode = nodes.Where((x) => x.Name == component.Name).First();

        foreach (var conn in component.OutputPortConnections) {
          var fromNodeOutput = fromNode.Outputs.Where((x) => x.Name == conn.Item1).First();
          var toNode = nodes.Where((x) => x.Name == conn.Item2.Name).First();
          var toNodeInput = toNode.Inputs.Where((x) => x.Name == conn.Item3).First();
          connections = connections.Add(new Connection(fromNode, fromNodeOutput, toNode, toNodeInput));
        }
      }

      return new Graph(nodes, connections);
    }

    private static FbpParseResult Parse(string fbpProgram) {
      return Parse(fbpProgram, null);
    }

    private static FbpParseResult Parse(string fbpProgram, Dictionary<string, object> initialData) {
      var components = new Dictionary<string, ParsedComponent>();
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

        ParsedComponent receiver = null;
        string receiverPort = null;

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
          var senderPort = match.Groups[2].Value;
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

          /*
          var metadata = ImmutableDictionary<string, string>.Empty.Add("value", match.Groups[2].Value);
          var component = new ParsedComponent("Value", "string", metadata);
          component.Setup();
          components[component.Name] = component;
          component.ConnectTo("Value", receiver, receiverPort);
          */
          receiver.SetInitialData(receiverPort, match.Groups[2].Value.Replace("\\'", "'"));

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

      return new FbpParseResult(ImmutableList<ParsedComponent>.Empty.AddRange(components.Values));
    }

    static ParsedComponent CreateOrRetrieveComponentFromString(string stringVersionOfComponent,
                                                               int sourceLineNumber,
                                                               Dictionary<string, ParsedComponent> components) {
      string componentName;
      string typeName = null;

      if (Regex.IsMatch(stringVersionOfComponent, COMPONENT_WITH_TYPE_DECLARATION)) {
        var match = Regex.Match(stringVersionOfComponent, COMPONENT_WITH_TYPE_DECLARATION);
        componentName = match.Groups[1].Value;
        typeName = match.Groups[2].Value;

        var metadata = ImmutableDictionary<string, string>.Empty;
        for(var i=3; i<match.Groups.Count; i++) {
          var matchMeta = Regex.Match(match.Groups[i].Value, COMPONENT_METADATA);
          metadata = metadata.Add(matchMeta.Groups[1].Value, matchMeta.Groups[2].Value);
        }

        if (components.ContainsKey(componentName)) {
          throw new ArgumentException(string.Format("Invalid input on line {0}, process '{1}' has already been declared", sourceLineNumber, componentName));
        }
        var component = new ParsedComponent(componentName, typeName, metadata);
        component.Setup();
        components[componentName] = component;
        return component;

      } else {
        componentName = stringVersionOfComponent;

        if (!components.ContainsKey(componentName)) {
          throw new ArgumentException(string.Format("Invalid input on line {0}, the process '{1}' has not been declared", sourceLineNumber, componentName));
        }
        return components[componentName];
      }
    }

    private class FbpParseResult {
      public readonly ImmutableList<ParsedComponent> Components;
      public FbpParseResult(ImmutableList<ParsedComponent> components) {
        this.Components = components;
      }
    }
    private class ParsedComponent {
      public readonly string Name;
      public readonly string Type;
      public readonly ImmutableDictionary<string, string> Metadata;
      public ImmutableList<string> InputPorts;
      public ImmutableList<string> OutputPorts;
      public ImmutableList<Tuple<string, ParsedComponent, string>> OutputPortConnections;
      public ImmutableDictionary<string, object> InputPortInitialDatas;
      public ParsedComponent(string name, string type, ImmutableDictionary<string, string> metadata) {
        this.Name = name;
        this.Type = type;
        this.Metadata = metadata;
        InputPorts = ImmutableList<string>.Empty;
        OutputPorts = ImmutableList<string>.Empty;
        OutputPortConnections = ImmutableList<Tuple<string, ParsedComponent, string>>.Empty;
        InputPortInitialDatas = ImmutableDictionary<string, object>.Empty;
      }
      internal void ConnectTo(string senderPort, ParsedComponent receiver, string receiverPort) {
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
        if (!InputPorts.Contains(receiverPort)) {
          InputPorts = InputPorts.Add(receiverPort);
        }

        InputPortInitialDatas = InputPortInitialDatas.Add(receiverPort, v);
      }

      internal void Setup() {
        //throw new NotImplementedException();
      }
    }
  }
}
