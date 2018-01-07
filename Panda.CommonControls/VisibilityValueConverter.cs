using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Panda.CommonControls
{
    public class VisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = System.Convert.ToBoolean(value ?? false);
            var invertResult = System.Convert.ToBoolean(parameter ?? false);
            isVisible = invertResult ? !isVisible : isVisible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}