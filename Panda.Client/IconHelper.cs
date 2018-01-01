using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Panda.Client
{
    /// <summary>
    ///     Class that assists with the loading of icons
    /// </summary>
    public static class IconHelper
    {
        /// <summary>
        ///     Gets an icon from a file path using windows shell
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>ImageSource for the icon for the icon at filePath</returns>
        // todo: add exception handling
        public static ImageSource IconFromFilePath(string filePath)
        {
            var img = Imaging.CreateBitmapSourceFromHIcon(
                Icon.ExtractAssociatedIcon(filePath).Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            return img;
        }
    }
}