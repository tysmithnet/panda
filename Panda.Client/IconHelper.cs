using System;
using System.Drawing;
using System.Runtime.InteropServices;
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
        public static ImageSource IconFromFilePath(string filePath)
        {
            try
            {
                var shinfo = new ShFileInfo();
                var hImgSmall = Win32.SHGetFileInfo(filePath, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                    Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON); // todo: handle big/small requests
                var icon = (Icon) Icon.FromHandle(shinfo.hIcon);
                var bmp = icon.ToBitmap();
                var img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                img.Freeze();
                return img;
            }
            catch (Exception)
            {
                var icon = new Icon("unknownfile32x32.ico");
                var bmp = icon.ToBitmap();
                var img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                img.Freeze();
                return img;
            }
        }

        /// <summary>
        ///     Win32 Api Shell File Info structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ShFileInfo
        {
            public readonly IntPtr hIcon;
            public readonly int iIcon;
            public readonly uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public readonly string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public readonly string szTypeName;
        }

        /// <summary>
        ///     Local win32 api wrapper
        /// </summary>
        private class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
            public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi,
                uint cbSizeFileInfo, uint uFlags);
        }
    }
}