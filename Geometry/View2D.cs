using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.Geometry {
  public class View2D {
    public readonly double CenterX;
    public readonly double CenterY;
    public readonly double Width;
    public readonly double Height;
    public readonly double Zoom;

    public View2D(double centerX, double centerY,
                  double width, double height,
                  double zoom) {
      this.CenterX = centerX;
      this.CenterY = centerY;
      this.Width = width;
      this.Height = height;
      this.Zoom = zoom;
    }

    public Point2 TopLeft {
      get {
        return new Point2(CenterX - Width / 2, CenterY - Height / 2);
      }
    }

    public Point2 TransformCCSToPCS(Point2 cartesianPointCCS) {
      double xVCS = (cartesianPointCCS.X - CenterX);
      double yVCS = (cartesianPointCCS.Y - CenterY);

      double xPCS = xVCS * Zoom + (Width / Zoom) / 2;
      double yPCS = yVCS * Zoom + (Height / Zoom) / 2;
      return new Point2(xPCS, yPCS);
    }

    public Point2 TransformPCSToCCS(Point2 cartesianPointPCS) {
      double xVCS = (cartesianPointPCS.X - (Width / Zoom) / 2) / Zoom;
      double yVCS = (cartesianPointPCS.Y - (Height / Zoom) / 2) / Zoom;

      double xCCS = xVCS + CenterX;
      double yCCS = yVCS + CenterY;
      return new Point2(xCCS, yCCS);
    }
  }
}
