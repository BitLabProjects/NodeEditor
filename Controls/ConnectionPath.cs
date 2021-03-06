﻿using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeEditor.Controls {

  class ConnectionPath : FrameworkElement {
    public ConnectionPath() {
      VisualStateManager.GoToElementState(this, "Normal", false);
    }

    private void updateVisualState() {
      if (mIsMouseOver) {
        if (mIsDragging) {
          VisualStateManager.GoToElementState(this, "Drag", true);
        } else {
          VisualStateManager.GoToElementState(this, "MouseOver", true);
        }
      } else {
        VisualStateManager.GoToElementState(this, "Normal", true);
      }
    }

    #region Mouse handling
    private bool mIsMouseOver;
    private bool mIsDragging;
    private Point mDragLastPoint;

    protected override void OnMouseEnter(MouseEventArgs e) {
      base.OnMouseEnter(e);
      mIsMouseOver = true;
      updateVisualState();
    }
    protected override void OnMouseLeave(MouseEventArgs e) {
      base.OnMouseLeave(e);
      mIsMouseOver = false;
      updateVisualState();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
      if (e.ClickCount == 2) {
        if (DoubleClickCommand != null) {
          if (DoubleClickCommand.CanExecute(DataContext)) {
            DoubleClickCommand.Execute(DataContext);
            e.Handled = true;
            return;
          }
        }
      }
    }
    #endregion

    #region Props
    public Brush Brush {
      get { return (Brush)GetValue(BrushProperty); }
      set { SetValue(BrushProperty, value); }
    }
    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.Register("Brush", typeof(Brush), typeof(ConnectionPath), new FrameworkPropertyMetadata((Brush)null, FrameworkPropertyMetadataOptions.AffectsRender));
    public bool IsShadow { get; set; }
    #endregion

    #region "Event commands"
    public ICommand DoubleClickCommand {
      get { return (ICommand)GetValue(DoubleClickCommandProperty); }
      set { SetValue(DoubleClickCommandProperty, value); }
    }
    public static readonly DependencyProperty DoubleClickCommandProperty =
        DependencyProperty.Register("DoubleClickCommand", typeof(ICommand), typeof(ConnectionPath), new PropertyMetadata(null));
    #endregion

    protected override void OnRender(DrawingContext drawingContext) {
      var parentPanel = VisualTreeUtils.GetParent<ConnectionsPanel>(this);
      if (parentPanel == null)
        return;

      var connection = DataContext as Connection;
      var layoutInfo = parentPanel.GetLayoutInfo(connection);
      var dx = (layoutInfo.toPoint.X - layoutInfo.fromPoint.X) / parentPanel.Zoom;
      var dy = (layoutInfo.toPoint.Y - layoutInfo.fromPoint.Y) / parentPanel.Zoom;

      const float margin = 10;

      Point[] points;
      if (dx < 0) {
        double marginY = 80;
        if (dy > 40 && dy < 140) {
          marginY = dy + 30;
        }
        var s = -Math.Sign(dy - marginY);
        // This is a connection from right to left
        points = new Point[] { new Point(margin, 0),
                               new Point(margin + margin / 2,           margin / 2),
                               new Point(margin + margin / 2, marginY - margin / 2),
                               new Point(             margin,              marginY),
                               new Point(        dx - margin,              marginY),
                               new Point(dx - margin - margin / 2, marginY - margin / 2 * s),
                               new Point(dx - margin - margin / 2,      dy + margin / 2 * s),
                               new Point(             dx - margin,                       dy),
                               new Point(                      dx,                       dy) };
      } else {
        var height = Math.Abs(dy);
        var s = Math.Sign(dy);
        var width = dx - 2 * margin;
        if (height > width) {
          var displacement = height - width;
          points = new Point[] { new Point(margin, 0),
                                 new Point(margin + width / 2, width / 2 * s),
                                 new Point(margin + width / 2, width / 2 * s + displacement * s),
                                 new Point(dx - margin, height * s),
                                 new Point(dx, height * s) };
        } else {
          var displacement = width - height;
          points = new Point[] { new Point(margin, 0),
                                 new Point(margin + height / 2, height / 2 * s),
                                 new Point(margin + height / 2 + displacement, height / 2 * s),
                                 new Point(dx - margin, height * s),
                                 new Point(dx, height * s) };
        }
      }

      var figure = new PathFigure();
      figure.IsClosed = false;
      figure.IsFilled = false;
      figure.StartPoint = new Point(0, 0);
      figure.Segments.Add(new PolyLineSegment(points, true));
      var geo = new PathGeometry();
      geo.Figures.Add(figure);

      const float outlineSize = 10;
      Pen shapeOutlinePen;
      if (IsShadow) {
        shapeOutlinePen = new Pen(Brush, outlineSize);
      } else {
        shapeOutlinePen = new Pen(getMultipliedAlpha255(Brush, 0.4f), outlineSize);
      }

      Pen shapeOutlinePen2 = new Pen(getMultipliedAlpha255(Brush, 1.5f), outlineSize);
      drawingContext.DrawGeometry(null, shapeOutlinePen, geo);
      if (!IsShadow) {
        Pen shapeInlinePen = new Pen(Brush, outlineSize - 2);
        drawingContext.DrawGeometry(null, shapeInlinePen, geo);
      }
      if (connection.FromNode != null) {
        drawingContext.DrawLine(shapeOutlinePen2, new Point(-margin, -2), new Point(0, -2));
      }
      if (connection.ToNode != null) {
        drawingContext.DrawLine(shapeOutlinePen2, new Point(dx + margin, dy - 2), new Point(dx, dy - 2));
      }
    }

    private Brush getMultipliedAlpha255(Brush input, float multiplier) {
      var BrushAsSolidColor = Brush as SolidColorBrush;
      if (BrushAsSolidColor != null) {
        return new SolidColorBrush(getMultipliedAlpha255(BrushAsSolidColor.Color, multiplier));
      } else {
        return input;
      }
    }

    private Color getMultipliedAlpha255(Color input, float multiplier) {
      var result = Color.Multiply(input, multiplier);
      result.A = 255;
      return result;
    }
  }
}
