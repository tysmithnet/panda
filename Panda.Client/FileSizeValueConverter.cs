using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Humanizer;

namespace Panda.Client
{
    /// <summary>
    /// Converter capable of turning file sizes into human readable formats
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class FileSizeValueConverter : IValueConverter
    {

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The converted value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var converted = System.Convert.ToUInt64(value);
            return Humanizer.Bytes.ByteSize.FromBytes(converted).Humanize("#.#"); // todo: make setting
        }

        /// <summary>
        /// Converts the value back to it's original form
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The converted value</returns>
        /// <exception cref="NotSupportedException">This should eventually be supported</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This is a lossy conversion, and thus 1 way"); // todo: implement parsing
        }
    }
}
