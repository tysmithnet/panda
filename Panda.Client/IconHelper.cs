using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Panda.Client
{
    public class IconHelper
    {
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