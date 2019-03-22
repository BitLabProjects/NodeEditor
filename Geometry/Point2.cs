using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Geometry {
  public struct Point2 {
    public double X { get; }
    public double Y { get; }

    public Point2(double x, double y) {
      this.X = x;
      this.Y = y;
    }

    public static Point2 operator -(Point2 point) {
      return new Point2(-point.X, -point.Y);
    }

    public static Point2 operator -(Point2 point1, Point2 point2) {
      return new Point2(point1.X - point2.X, point1.Y - point2.Y);
    }
    public static Point2 operator +(Point2 point1, Point2 point2) {
      return new Point2(point1.X + point2.X, point1.Y + point2.Y);
    }

    public static Point2 operator *(Point2 point, double value) {
      return new Point2(point.X * value, point.Y * value);
    }
    public static Point2 operator /(Point2 point, double value) {
      return new Point2(point.X / value, point.Y / value);
    }
  }
}
