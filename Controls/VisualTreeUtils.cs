using NodeEditor.App.Commands;
using NodeEditor.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeEditor.Controls {
  class VisualTreeUtils {
    public static T GetDataContextOnParents<T>(FrameworkElement control) where T : class {
      while (control != null) {
        var dc = control.DataContext as T;
        if (dc != null) return dc;

        var parent = VisualTreeHelper.GetParent(control);
        if (parent == null) {
          parent = LogicalTreeHelper.GetParent(control);
        }

        control = parent as FrameworkElement;
      }

      return null;
    }

    public static FrameworkElement GetLastParentWithDataContextOfType<T>(DependencyObject control) where T : class {
      return GetLastParent<FrameworkElement>(control, (fe) => (fe.DataContext as T) != null);
    }

    public static T GetLastParent<T>(DependencyObject control, Predicate<T> predicate) where T : class {
      T lastParent = null;
      while (control != null) {
        var parent = VisualTreeHelper.GetParent(control);
        if (parent == null) {
          parent = LogicalTreeHelper.GetParent(control);
        }

        var parentAsT = parent as T;
        if (parentAsT != null) {
          if (predicate(parentAsT)) {
            lastParent = parentAsT;
          } else {
            return lastParent;
          }
        } else {
          return lastParent;
        }

        control = parent;
      }

      return null;
    }

    public static T GetParent<T>(DependencyObject control) where T : class {
      while (control != null) {
        var parent = VisualTreeHelper.GetParent(control);
        if (parent == null) {
          parent = LogicalTreeHelper.GetParent(control);
        }

        var parentAsT = parent as T;
        if (parentAsT != null) return parentAsT;
        
        control = parent;
      }

      return null;
    }

    public static FrameworkElement HitTestWithDataContext<T>(Visual reference, Point2 point) where T : class {
      FrameworkElement result = null;
      HitTestFilterCallback filterCallback = (DependencyObject candidate) => {
        var fe = candidate as FrameworkElement;
        if (fe == null)
          return HitTestFilterBehavior.ContinueSkipSelf;

        var dc = fe.DataContext as T;
        if (dc == null) {
          return HitTestFilterBehavior.ContinueSkipSelf;
        }
        return HitTestFilterBehavior.Continue;
      };
      HitTestResultCallback resultCallback = (HitTestResult hitTestResult) => {
        result = hitTestResult.VisualHit as FrameworkElement;
        return HitTestResultBehavior.Stop;
      };

      VisualTreeHelper.HitTest(reference, filterCallback, resultCallback, new PointHitTestParameters(new Point(point.X, point.Y)));

      return result;
    }

    public static T GetVisualParent<T>(DependencyObject visual) where T : class {
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

    public static Panel GetItemsHost(ItemsControl itemsControl) {
      return typeof(ItemsControl).InvokeMember("ItemsHost",
                                               BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance,
                                               null, itemsControl, null) as Panel;
    }
  }

  class AttachedProps {
    #region CommandManager
    public static ICommandManager GetCommandManager(DependencyObject obj) {
      return (ICommandManager)obj.GetValue(CommandManagerProperty);
    }
    public static void SetCommandManager(DependencyObject obj, ICommandManager value) {
      obj.SetValue(CommandManagerProperty, value);
    }
    public static readonly DependencyProperty CommandManagerProperty =
        DependencyProperty.RegisterAttached("CommandManager", typeof(ICommandManager), typeof(AttachedProps), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
    #endregion

    #region RenderTransformOrigin
    public static int GetRenderTransformOrigin(DependencyObject obj) {
      return (int)obj.GetValue(RenderTransformOriginProperty);
    }
    public static void SetRenderTransformOrigin(DependencyObject obj, int value) {
      obj.SetValue(RenderTransformOriginProperty, value);
    }
    public static readonly DependencyProperty RenderTransformOriginProperty =
        DependencyProperty.RegisterAttached("RenderTransformOrigin", typeof(int), typeof(AttachedProps), new PropertyMetadata(0, RenderTransformOrigin_PropertyChanged));

    private static void RenderTransformOrigin_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var fe = d as FrameworkElement;
      fe.RenderTransform = new TranslateTransform(0, 0);
      Action fixRT = () => {
        var tt = fe.RenderTransform as TranslateTransform;
        double fx = 0, fy = 0;
        switch (GetRenderTransformOrigin(fe)) {
          case 0: case 1: case 2: fx = 0; break;
          case 3: case 4: case 5: fx = -0.5; break;
          case 6: case 7: case 8: fx = -1; break;
        }
        switch (GetRenderTransformOrigin(fe)) {
          case 0: case 3: case 6: fy = 0; break;
          case 1: case 4: case 7: fy = -0.5; break;
          case 2: case 5: case 8: fy = -1; break;
        }
        tt.X = fe.ActualWidth * fx;
        tt.Y = fe.ActualHeight * fy;
      };
      fe.SizeChanged += (s1, e1) => {
        fixRT();
      };
      fixRT();
    }
    #endregion
  }
}
