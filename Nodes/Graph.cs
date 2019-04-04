using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  public class Graph {
    public ImmutableList<Node> Nodes { get; }
    public ImmutableList<Connection> Connections { get; }

    public Graph(ImmutableList<Node> nodes,
                 ImmutableList<Connection> connections) {
      this.Nodes = nodes;
      this.Connections = connections;
    }

    #region Query
    public ImmutableArray<Connection> GetAllConnectionsForNodeOutput(Node node, NodeOutput nodeOutput) {
      return Connections.Where((c) => c.FromNode == node && c.FromNodeOutput == nodeOutput).ToImmutableArray();
    }
    #endregion

    #region Manipulation
    public Graph RemoveConnection(Connection c) {
      return new Graph(Nodes, Connections.Remove(c));
    }
    public Graph AddConnection(Connection c) {
      return new Graph(Nodes, Connections.Add(c));
    }
    public Graph AddNode(Node n) {
      return new Graph(Nodes.Add(n), Connections);
    }
    public Graph ReplaceNode(Node oldN, Node newN) {
      var newConnections = Connections.ConvertAll<Connection>((c) => c.ReplaceNode(oldN, newN));
      return new Graph(Nodes.Replace(oldN, newN), newConnections);
    }
    #endregion
  }
}
