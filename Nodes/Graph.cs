using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Nodes {
  class Graph {
    public ImmutableList<Node> Nodes { get; }
    public ImmutableList<Connection> Connections { get; }

    public Graph(ImmutableList<Node> nodes,
                 ImmutableList<Connection> connections) {
      this.Nodes = nodes;
      this.Connections = connections;
    }

    #region Manipulation
    public Graph RemoveConnection(Connection c) {
      return new Graph(Nodes, Connections.Remove(c));
    }
    public Graph AddConnection(Connection c) {
      return new Graph(Nodes, Connections.Add(c));
    }
    #endregion
  }
}
