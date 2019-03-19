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

namespace NodeEditor.Controls
{

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

      if (CaptureMouse()) {
        mIsDragging = true;
        mDragLastPoint = e.GetPosition(this);
        e.Handled = true;

        updateVisualState();
      }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      if (mIsDragging) {
        var newPoint = e.GetPosition(this);
        var delta = newPoint - mDragLastPoint;
        mDragLastPoint = newPoint;

        e.Handled = true;
      }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
      if (mIsDragging) {
        mIsDragging = false;
        ReleaseMouseCapture();
        e.Handled = true;

        updateVisualState();
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

      var figure = new PathFigure();
      figure.IsClosed = false;
      figure.IsFilled = false;
      figure.StartPoint = new Point(0, 0);

      const float margin = 10;
      var height = dy;
      var width = dx - 2 * margin;
      Point[] points;
      if (height > width) {
        var displacement = height - width;
        points = new Point[] { new Point(margin, 0),
                               new Point(margin + width / 2, width / 2),
                               new Point(margin + width / 2, width / 2 + displacement),
                               new Point(dx - margin, dy),
                               new Point(dx, dy) };
      } else {
        var displacement = width - height;
        points = new Point[] { new Point(margin, 0),
                               new Point(margin + height / 2, height / 2),
                               new Point(margin + height / 2 + displacement, height / 2),
                               new Point(dx - margin, dy),
                               new Point(dx, dy) };
      }

      figure.Segments.Add(new PolyLineSegment(points, true));
      var geo = new PathGeometry();
      geo.Figures.Add(figure);

      const float outlineSize = 10;
      Pen shapeOutlinePen = new Pen(Brush, outlineSize);

      Pen shapeOutlinePen2;
      var BrushAsSolidColor = Brush as SolidColorBrush;
      if (BrushAsSolidColor != null) {
        shapeOutlinePen2 = new Pen(new SolidColorBrush(getMultipliedAlpha255(BrushAsSolidColor.Color)), outlineSize);
      } else {
        shapeOutlinePen2 = shapeOutlinePen;
      }
      drawingContext.DrawGeometry(null, shapeOutlinePen, geo);
      if (connection.FromNode != null) {
        drawingContext.DrawLine(shapeOutlinePen2, new Point(-margin, -2), new Point(0, -2));
      }
      if (connection.ToNode != null) {
        drawingContext.DrawLine(shapeOutlinePen2, new Point(dx + margin, dy - 2), new Point(dx, dy - 2));
      }
    }

    private Color getMultipliedAlpha255(Color input) {
      var result = Color.Multiply(input, 1.5f);
      result.A = 255;
      return result;
    }
  }
}
