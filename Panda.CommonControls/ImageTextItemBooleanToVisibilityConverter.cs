using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Panda.CommonControls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    internal class ImageTextItemBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>Visible if value is true, otherwise Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {         
            var isVisible = System.Convert.ToBoolean(value ?? false);
            var invertResult = System.Convert.ToBoolean(parameter ?? false);
            isVisible = invertResult ? !isVisible : isVisible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>true if value is Visible, otherwise false</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Visibility? == Visibility.Visible;
        }
    }
}