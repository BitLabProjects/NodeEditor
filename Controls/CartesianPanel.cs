using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NodeEditor.Geometry;
using NodeEditor.Nodes;

namespace NodeEditor.Controls {
  class CartesianPanel: Panel {
    private Dictionary<Node, Rect> mNodeSizeInfos;

    public CartesianPanel() {
      mNodeSizeInfos = new Dictionary<Node, Rect>();
    }

    public Rect GetNodeSizeInfo(Node node) {
      Rect result;
      if (!mNodeSizeInfos.TryGetValue(node, out result)) {
        //Error
        result = new Rect(0, 0, 1, 1);
      }
      return result;
    }

    #region Coordinates attached properties
    public static double GetX(DependencyObject obj) {
      return (double)obj.GetValue(XProperty);
    }
    public static void SetX(DependencyObject obj, double value) {
      obj.SetValue(XProperty, value);
    }
    public static readonly DependencyProperty XProperty =
        DependencyProperty.RegisterAttached("X", typeof(double), typeof(CartesianPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static double GetY(DependencyObject obj) {
      return (double)obj.GetValue(YProperty);
    }
    public static void SetY(DependencyObject obj, double value) {
      obj.SetValue(YProperty, value);
    }
    public static readonly DependencyProperty YProperty =
        DependencyProperty.RegisterAttached("Y", typeof(double), typeof(CartesianPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
    #endregion

    #region Props
    public double Zoom {
      get { return (double)GetValue(ZoomProperty); }
      set { SetValue(ZoomProperty, value); }
    }
    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register("Zoom", typeof(double), typeof(CartesianPanel), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public View2D Viewport {
      get { return (View2D)GetValue(ViewportProperty); }
      set { SetValue(ViewportProperty, value); }
    }
    public static readonly DependencyProperty ViewportProperty =
        DependencyProperty.Register("Viewport", typeof(View2D), typeof(CartesianPanel), new FrameworkPropertyMetadata(new View2D(0, 0, 100, 100, 1), FrameworkPropertyMetadataOptions.AffectsMeasure));
    #endregion

    protected override Size MeasureOverride(Size availableSize) {
      // Just measure every child, giving it infinite space.
      // The panel itself occupies the whole area given.

      var view2D = this.Viewport;

      var NodeEditorControl = VisualTreeUtils.GetVisualParent<NodeEditorControl>(this);
      var itemsControl = ItemsControl.GetItemsOwner(this) as ItemsControl;
      var generator = itemsControl.ItemContainerGenerator;

      Size childConstraint = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

      foreach (UIElement child in InternalChildren) {
        if (child == null) { continue; }
        child.Measure(childConstraint);

        var node = generator.ItemFromContainer(child) as Node;
        //var positionCCS = new Point2(GetX(child), GetY(child));
        var positionCCS = NodeEditorControl.GetNodePositionMaybePreview(node);
        var pointPCS = view2D.TransformCCSToPCS(positionCCS);
        mNodeSizeInfos[node] = new Rect(pointPCS.X, pointPCS.Y, child.DesiredSize.Width, child.DesiredSize.Height);
      }

      return new Size();
    }

    protected override Size ArrangeOverride(Size finalSize) {
      var zoom = this.Zoom;
      var view2D = this.Viewport;

      var itemsControl = ItemsControl.GetItemsOwner(this) as ItemsControl;
      var generator = itemsControl.ItemContainerGenerator;

      foreach (UIElement child in InternalChildren) {
        if (child == null) { continue; }

        var node = generator.ItemFromContainer(child) as Node;
        var sizeInfo = mNodeSizeInfos[node];

        child.RenderTransform = new ScaleTransform(zoom, zoom);
        child.Arrange(sizeInfo);
      }

      return finalSize;
    }
  }
}
