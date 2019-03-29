using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Fbp {
  public static class FbpWriter {
    public static void Write(Graph graph, string fbpFullFileName) {
      using (var writer = new StreamWriter(fbpFullFileName, append: false, encoding: Encoding.UTF8)) {
        var declaredNodes = new HashSet<Node>();

        for(int i=0; i<graph.Nodes.Count; i++) {
          var node = graph.Nodes[i];
          for (int j = 0; j < node.Inputs.Length; j++) {
            var nodeInput = node.Inputs[j];
            if (nodeInput.InitialData != null) {
              string nodeDeclaration = "";
              if (!declaredNodes.Contains(node)) {
                nodeDeclaration = $"({node.Type}:x={node.Position.X},y={node.Position.Y})";
              }
              writer.WriteLine($"{getInitialDataAsString(nodeInput.InitialData)} -> {nodeInput.Name} {node.Name}{nodeDeclaration}");
            }
          }
        }
      }
    }

    private static string getInitialDataAsString(object initialData) {
      if (initialData.GetType() == typeof(string)) {
        return $"'{((string)initialData).Replace("'", "\\'")}'";
      }
      throw new ArgumentException("Unknown type of initial data");
    }
  }
}
