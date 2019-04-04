using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NodeEditor.UI.Converters {
  public class BooleanToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var trueValue = (parameter as string ?? "").ToLowerInvariant() != "not";

      bool isVisible;
      if ((bool)value) {
        isVisible = trueValue;
      } else {
        isVisible = !trueValue;
      }
      return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
