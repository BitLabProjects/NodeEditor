using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace NodeEditor.Controls {
  struct ConnectionPathLayoutInfo {
    public Point fromPoint;
    public Point toPoint;
  }

  class ConnectionsPanel : CartesianPanel {
    public ConnectionsPanel() {
      mLayoutInfo = new Dictionary<Connection, ConnectionPathLayoutInfo>();
    }

    protected override Size MeasureOverride(Size availableSize) {
      // Just measure every child, giving it infinite space.
      // The panel itself occupies the whole area given.

      Size childConstraint = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

      foreach (UIElement child in InternalChildren) {
        if (child == null) { continue; }
        child.Measure(childConstraint);
      }

      return new Size();
    }

    private Dictionary<Connection, ConnectionPathLayoutInfo> mLayoutInfo;
    public ConnectionPathLayoutInfo GetLayoutInfo(Connection connection) {
      if (mLayoutInfo.ContainsKey(connection)) {
        return mLayoutInfo[connection];
      }
      return new ConnectionPathLayoutInfo();
    }

    protected override Size ArrangeOverride(Size finalSize) {
      var NodeEditorControl = VisualTreeUtils.GetVisualParent<NodeEditorControl>(this);

      ItemsControl nodesItemsControl = null;
      CartesianPanel nodesCartesianPanel = null;
      if (NodeEditorControl != null) {
        nodesItemsControl = NodeEditorControl.GetNodesItemsControl();
        nodesCartesianPanel = VisualTreeUtils.GetItemsHost(nodesItemsControl) as CartesianPanel;
      }

      if (nodesItemsControl == null) {
        foreach (UIElement child in InternalChildren) {
          if (child == null) { continue; }
          child.Arrange(new Rect(new Point(0, 0), new Size(0, 0)));
        }

      } else {
        var zoom = this.Zoom;

        var connectionsItemsControl = ItemsControl.GetItemsOwner(this) as ItemsControl;
        var connectionsGenerator = connectionsItemsControl == null ? null : connectionsItemsControl.ItemContainerGenerator;
        var nodesGenerator = nodesItemsControl.ItemContainerGenerator;
        foreach (UIElement child in InternalChildren) {
          if (child == null) { continue; }

          Connection connection;
          if (connectionsGenerator != null) {
            connection= connectionsGenerator.ItemFromContainer(child) as Connection;
          } else if (child as ContentControl != null) {
            connection = (child as ContentControl).Content as Connection;
          } else if ((child as FrameworkElement).DataContext as Connection != null) {
            connection = (child as FrameworkElement).DataContext as Connection;
          } else {
            // ???
            connection = null;
          }

          var verticalOutputOffset = 47.0 + 20.0 * connection.FromNode.GetOutputIndex(connection.FromNodeOutput);

          var fromNodeContainer = nodesGenerator.ContainerFromItem(connection.FromNode) as FrameworkElement;
          if (!fromNodeContainer.IsArrangeValid) {
            this.InvalidateArrange();
            return finalSize;
          }
          var fromNodeOrigin = fromNodeContainer.TranslatePoint(new Point(0, 0), nodesItemsControl);
          var fromNodeSize = nodesCartesianPanel.GetNodeSizeInfo(connection.FromNode).Size;
          fromNodeSize = new Size(fromNodeSize.Width * zoom,
                                  fromNodeSize.Height * zoom);
          Point fromPoint = new Point(fromNodeOrigin.X + fromNodeSize.Width,
                                      fromNodeOrigin.Y + verticalOutputOffset * zoom);

          Point toNodeOrigin;
          double verticalInputOffset;
          if (connection.ToNode != null) {
            var toNodeContainer = nodesGenerator.ContainerFromItem(connection.ToNode) as FrameworkElement;
            if (!toNodeContainer.IsArrangeValid) {
              this.InvalidateArrange();
              return finalSize;
            }
            toNodeOrigin = toNodeContainer.TranslatePoint(new Point(0, 0), nodesItemsControl);
            //var toNodeSize = nodesCartesianPanel.GetNodeSizeInfo(connection.ToNode).Size;
            //toNodeSize = new Size(toNodeSize.Width * zoom,
            //                      toNodeSize.Height * zoom);

            verticalInputOffset = 47.0 + 20.0 * connection.ToNode.GetInputIndex(connection.ToNodeInput);

          } else {
            //toNodeOrigin = NodeEditorControl.previewMousePosition;
            toNodeOrigin = Mouse.GetPosition(this);
            verticalInputOffset = 0;
            //child.RenderTransform = new ScaleTransform(zoom, zoom);
            //child.Arrange(new Rect(childOrigin, new Size(100, 50)));
            //continue;
          }

          var toPoint = new Point(toNodeOrigin.X, toNodeOrigin.Y + verticalInputOffset * zoom);

          mLayoutInfo[connection] = new ConnectionPathLayoutInfo() { fromPoint = fromPoint, toPoint = toPoint };

          // We're making an assumption on the visual tree here.
          // Invalidate the visual of the descendant, assuming it's a ConnectionPath which needs to do so
          (VisualTreeHelper.GetChild((child as ContentPresenter), 0) as UIElement).InvalidateVisual();

          child.RenderTransform = new ScaleTransform(zoom, zoom);
          //child.Arrange(new Rect(fromPoint, childSize));
          child.Arrange(new Rect(fromPoint, new Size(1, 1)));
        }
      }

      return finalSize;
    }
  }
}
