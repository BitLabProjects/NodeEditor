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
  class ConnectionsPanel : CartesianPanel {
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

    protected override Size ArrangeOverride(Size finalSize) {
      var nodesItemsControl = GetNodesItemsControl();
      var nodesCartesianPanel = GetNodesCartesianPanel();
      if (nodesItemsControl == null) {
        foreach (UIElement child in InternalChildren) {
          if (child == null) { continue; }
          child.Arrange(new Rect(new Point(0, 0), new Size(0, 0)));
        }

      } else {
        var zoom = this.Zoom;

        var connectionsItemsControl = ItemsControl.GetItemsOwner(this) as ItemsControl;
        var connectionsGenerator = connectionsItemsControl.ItemContainerGenerator;
        var nodesGenerator = nodesItemsControl.ItemContainerGenerator;
        foreach (UIElement child in InternalChildren) {
          if (child == null) { continue; }

          var connection = connectionsGenerator.ItemFromContainer(child) as Connection;

          var fromNodeContainer = nodesGenerator.ContainerFromItem(connection.FromNode) as FrameworkElement;
          if (!fromNodeContainer.IsArrangeValid) {
            this.InvalidateArrange();
            return finalSize;
          }
          var fromNodeOrigin = fromNodeContainer.TranslatePoint(new Point(0, 0), nodesItemsControl);
          var fromNodeSize = nodesCartesianPanel.GetNodeSizeInfo(connection.FromNode).Size;
          fromNodeSize = new Size(fromNodeSize.Width * zoom,
                                  fromNodeSize.Height * zoom);

          var toNodeContainer = nodesGenerator.ContainerFromItem(connection.ToNode) as FrameworkElement;
          if (!toNodeContainer.IsArrangeValid) {
            this.InvalidateArrange();
            return finalSize;
          }
          var toNodeOrigin = toNodeContainer.TranslatePoint(new Point(0, 0), nodesItemsControl);
          var toNodeSize = nodesCartesianPanel.GetNodeSizeInfo(connection.ToNode).Size;
          toNodeSize = new Size(toNodeSize.Width * zoom,
                                toNodeSize.Height * zoom);

          Point childOrigin;
          Size childSize;

          var verticalOutputOffset = 37.0 + 20.0 * connection.FromNode.GetOutputIndex(connection.FromNodeOutput);
          var verticalInputOffset = 37.0 + 20.0 * connection.ToNode.GetInputIndex(connection.ToNodeInput);
          childOrigin = new Point(fromNodeOrigin.X + fromNodeSize.Width,
                                  fromNodeOrigin.Y + verticalOutputOffset * zoom);
          childSize = new Size(toNodeOrigin.X - childOrigin.X,
                               toNodeOrigin.Y + verticalInputOffset * zoom - childOrigin.Y);

          childSize = new Size(childSize.Width / zoom,
                               childSize.Height / zoom);

          child.RenderTransform = new ScaleTransform(zoom, zoom);
          child.Arrange(new Rect(childOrigin, childSize));
        }
      }

      return finalSize;
    }

    private ItemsControl GetNodesItemsControl() {
      var NodeEditorControl = GetVisualParent<NodeEditorControl>(this);
      if (NodeEditorControl == null) {
        return null;
      }

      return NodeEditorControl.GetNodesItemsControl();
    }

    private CartesianPanel GetNodesCartesianPanel() {
      var NodeEditorControl = GetVisualParent<NodeEditorControl>(this);
      if (NodeEditorControl == null) {
        return null;
      }

      return NodeEditorControl.GetNodesCartesianPanel();
    }

    private T GetVisualParent<T>(DependencyObject visual) where T : class {
      var curr = visual;
      while (curr != null) {
        var parent = VisualTreeHelper.GetParent(curr);
        var parentAsT = parent as T;
        if (parentAsT != null) {
          return parentAsT;
        }
        curr = parent;
      }
      return null;
    }
  }
}
