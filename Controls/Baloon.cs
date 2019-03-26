using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeEditor.Controls {
  public class Baloon : ContentControl {
    static Baloon() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Baloon), new FrameworkPropertyMetadata(typeof(Baloon)));
    }

    public double ArrowSize {
      get { return (double)GetValue(ArrowSizeProperty); }
      set { SetValue(ArrowSizeProperty, value); }
    }
    public static readonly DependencyProperty ArrowSizeProperty =
        DependencyProperty.Register("ArrowSize", typeof(double), typeof(Baloon), new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    protected override Size MeasureOverride(Size constraint) {
      int count = this.VisualChildrenCount;

      Size sizeForChild;
      if (double.IsInfinity(constraint.Width)) {
        sizeForChild = constraint;
      } else {
        sizeForChild = new Size(Math.Max(0, constraint.Width - ArrowSize), constraint.Height);
      }

      if (count > 0) {
        UIElement child = (UIElement)(this.GetVisualChild(0));
        if (child != null) {
          child.Measure(sizeForChild);
          return new Size(child.DesiredSize.Width + ArrowSize, child.DesiredSize.Height);
        }
      }

      return new Size(0.0, 0.0);
    }

    protected override void OnRender(DrawingContext drawingContext) {
      var pen = new Pen(BorderBrush, BorderThickness.Left);
      if (RenderSize.Width < ArrowSize) {
        drawingContext.DrawRectangle(Background, pen, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
      } else {
        Point[] points;
        points = new Point[] { new Point(RenderSize.Width - ArrowSize, 0),
                               new Point(RenderSize.Width, RenderSize.Height / 2),
                               new Point(RenderSize.Width - ArrowSize, RenderSize.Height),
                               new Point(0, RenderSize.Height)};

        var figure = new PathFigure();
        figure.IsClosed = true;
        figure.IsFilled = true;
        figure.StartPoint = new Point(0, 0);
        figure.Segments.Add(new PolyLineSegment(points, true));
        var geo = new PathGeometry();
        geo.Figures.Add(figure);

        drawingContext.DrawGeometry(Background, pen, geo);
      }
    }
  }
}
