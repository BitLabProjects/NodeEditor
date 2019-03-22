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
  }
}
