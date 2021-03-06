﻿using System;
using System.Globalization;
using System.Windows.Data;
using Humanizer;
using Humanizer.Bytes;

namespace Panda.Client
{
    /// <summary>
    ///     Converter capable of turning file sizes into human readable formats
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public sealed class FileSizeValueConverter : IValueConverter
    {
        /// <summary>
        ///     Converts the specified value.
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
            return ByteSize.FromBytes(converted).Humanize("#.#");
        }

        /// <summary>
        ///     Converts the value back to it's original form
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The converted value</returns>
        /// <exception cref="NotSupportedException">This should eventually be supported</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This is a lossy conversion, and thus 1 way");
        }
    }
}