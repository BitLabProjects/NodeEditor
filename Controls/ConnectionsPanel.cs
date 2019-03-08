using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        foreach(UIElement child in InternalChildren) {
          if (child == null) { continue; }

          var connection = connectionsGenerator.ItemFromContainer(child) as Connection;

          var fromNodeContainer = nodesGenerator.ContainerFromItem(connection.FromNode) as FrameworkElement;
          if (!fromNodeContainer.IsArrangeValid) {
            this.InvalidateArrange();
            return finalSize;
          }
          var fromNodeOrigin = fromNodeContainer.TranslatePoint(new Point(0, 0), nodesItemsControl);
          //var fromNodeSize = new Size(fromNodeContainer.RenderSize.Width * zoom,
          //                            fromNodeContainer.RenderSize.Height * zoom);
          var fromNodeSize = nodesCartesianPanel.GetNodeSizeInfo(connection.FromNode).Size;
          fromNodeSize = new Size(fromNodeSize.Width * zoom,
                                  fromNodeSize.Height * zoom);

          var toNodeContainer = nodesGenerator.ContainerFromItem(connection.ToNode) as FrameworkElement;
          if (!toNodeContainer.IsArrangeValid) {
            this.InvalidateArrange();
            return finalSize;
          }
          var toNodeOrigin = toNodeContainer.TranslatePoint(new Point(0, 0), nodesItemsControl);
          //var toNodeSize = new Size(toNodeContainer.RenderSize.Width * zoom,
          //                          toNodeContainer.RenderSize.Height * zoom);
          var toNodeSize = nodesCartesianPanel.GetNodeSizeInfo(connection.ToNode).Size;
          toNodeSize = new Size(toNodeSize.Width * zoom,
                                toNodeSize.Height * zoom);

          Point childOrigin;
          Size childSize;
          //childOrigin = new Point(Math.Min(fromNodeOrigin.X, toNodeOrigin.X),
          //                        Math.Min(fromNodeOrigin.Y, toNodeOrigin.Y));
          //var childBottomRight = new Point(Math.Max(fromNodeOrigin.X + fromNodeSize.Width, toNodeOrigin.X + toNodeSize.Width),
          //                                 Math.Max(fromNodeOrigin.Y + fromNodeSize.Height, toNodeOrigin.Y + toNodeSize.Height));
          //childSize = new Size((childBottomRight.X - childOrigin.X) / zoom,
          //                     (childBottomRight.Y - childOrigin.Y) / zoom);

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

    private T GetVisualParent<T>(DependencyObject visual) where T: class {
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

  class CustomPath: UIElement {
    private DrawingVisual mVisual;
    public CustomPath() {
      DrawingVisual dv = new DrawingVisual();
      

      Effect = new DropShadowEffect();

      mVisual = dv;
      AddVisualChild(dv);
    }

    protected override void ArrangeCore(Rect finalRect) {
      base.ArrangeCore(finalRect);

      var figure = new PathFigure();
      figure.IsClosed = false;
      figure.IsFilled = false;
      figure.StartPoint = new Point(0, 0);

      var margin = 10;
      var height = finalRect.Height;
      var width = finalRect.Width - 2 * margin;
      Point[] points;
      if (height > width) {
        var displacement = height - width;
        points = new Point[] { new Point(margin, 0),
                               new Point(margin + width / 2, width / 2),
                               new Point(margin + width / 2, width / 2 + displacement),
                               new Point(finalRect.Width - margin, finalRect.Height),
                               new Point(finalRect.Width, finalRect.Height) };
      } else {
        var displacement = width - height;
        points = new Point[] { new Point(margin, 0),
                               new Point(margin + height / 2, height / 2),
                               new Point(margin + height / 2 + displacement, height / 2),
                               new Point(finalRect.Width - margin, finalRect.Height),
                               new Point(finalRect.Width, finalRect.Height) };
      }

      //figure.Segments.Add(new PolyBezierSegment(points, true));
      figure.Segments.Add(new PolyLineSegment(points, true));
      var geo = new PathGeometry();
      geo.Figures.Add(figure);

      //Pen shapeOutlinePen = new Pen(new SolidColorBrush(Color.FromArgb(255, 114, 234, 114)), 12);
      Pen shapeOutlinePen = new Pen(new SolidColorBrush(Color.FromArgb(255, 100, 200, 100)), 10);
      Pen shapeInlinePen = new Pen(new SolidColorBrush(Color.FromArgb(255, 152, 251, 152)), 6);
      Pen shapeOutlinePen2 = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)), 10);
      Pen shapeInlinePen2 = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 230, 0)), 6);
      shapeOutlinePen.Freeze();
      shapeInlinePen.Freeze();
      shapeOutlinePen2.Freeze();
      shapeInlinePen2.Freeze();
      // Obtain a DrawingContext from 
      // the DrawingGroup.
      using (DrawingContext dc = mVisual.RenderOpen()) {
        dc.DrawGeometry(Brushes.Green, shapeOutlinePen, geo);
        dc.DrawGeometry(Brushes.Green, shapeInlinePen, geo);

        dc.DrawLine(shapeOutlinePen2, new Point(-10, -2), new Point(0, -2));
        dc.DrawLine(shapeInlinePen2, new Point(-10, -2), new Point(0, -2));
        dc.DrawLine(shapeOutlinePen2, new Point(finalRect.Width + 10, finalRect.Height - 2), new Point(finalRect.Width, finalRect.Height - 2));
        dc.DrawLine(shapeInlinePen2, new Point(finalRect.Width + 10, finalRect.Height - 2), new Point(finalRect.Width, finalRect.Height - 2));
      }
    }

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) {
      return mVisual;
    }
  }
}
