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
  }
}
