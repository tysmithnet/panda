using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Panda.Client
{
    /// <summary>
    ///     Class that assists with the loading of icons
    /// </summary>
    public static class ShellHelper
    {
        /// <summary>
        ///     The large icon cache
        /// </summary>
        private static readonly MemoryCache LargeIconCache = new MemoryCache(typeof(ShellHelper).FullName + "_large");

        /// <summary>
        ///     The small icon cache
        /// </summary>
        private static readonly MemoryCache SmallIconCache = new MemoryCache(typeof(ShellHelper).FullName + "_small");

        /// <summary>
        ///     The unknown file large
        /// </summary>
        private static readonly ImageSource UnknownFileLarge;

        /// <summary>
        ///     The unknown file small
        /// </summary>
        private static readonly ImageSource UnknownFileSmall;

        /// <summary>
        ///     Initializes the <see cref="ShellHelper" /> class.
        /// </summary>
        static ShellHelper()
        {
            var shinfo = new NativeMethods.ShFileInfo();
            var hImgSmall = NativeMethods.SHGetFileInfo("unknownfile32x32.ico", 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                NativeMethods.SHGFI_ICON | IconSize.Large.ToIconFlag());
            var icon = Icon.FromHandle(shinfo.hIcon);
            var bmp = icon.ToBitmap();
            var img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            UnknownFileLarge = img;

            hImgSmall = NativeMethods.SHGetFileInfo("unknownfile16x16.ico", 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                NativeMethods.SHGFI_ICON | IconSize.Small.ToIconFlag());
            icon = Icon.FromHandle(shinfo.hIcon);
            bmp = icon.ToBitmap();
            img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            UnknownFileSmall = img;
        }

        public static string GetDisplayName(string filePath)
        {
            StringBuilder result = new StringBuilder();
            int cch = 0;
            int pidsRes = 0;
            NativeMethods.SHGetLocalizedName(filePath, result, ref cch, out pidsRes);
            return result.ToString();
        }

        /// <summary>
        ///     To the icon flag.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private static uint ToIconFlag(this IconSize size)
        {
            return (uint) size;
        }

        /// <summary>
        ///     Gets the fallback icon.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">size</exception>
        public static ImageSource GetFallbackIcon(IconSize size)
        {
            switch (size)
            {
                case IconSize.Large:
                    return UnknownFileLarge;
                case IconSize.Small:
                    return UnknownFileSmall;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(size)} is not a valid IconSize");
            }
        }

        public static ShellFileInfo GetShellFileInfo(string filePath)
        {
            var shinfo = new NativeMethods.ShFileInfo();
            var hImgSmall = NativeMethods.SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                NativeMethods.SHGFI_ICON | IconSize.Small.ToIconFlag());
            var icon = Icon.FromHandle(shinfo.hIcon);
            var bmp = icon.ToBitmap();
            var img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            var fvi = FileVersionInfo.GetVersionInfo(filePath);
            string displayName = Path.GetFileName(filePath);
            if (!string.IsNullOrWhiteSpace(fvi.ProductName))
                displayName = fvi.ProductName;
            return new ShellFileInfo
            {
                Icon = img,
                DisplayName = displayName,
                Description = fvi.FileDescription,
                FilePath = filePath
            };
        }


        /// <summary>
        ///     Gets an icon from a file path using windows shell
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>ImageSource for the icon for the icon at filePath</returns>
        public static ImageSource IconFromFilePath(string filePath, IconSize size, bool shouldFallBack = true)
        {
            MemoryCache memoryCache;
            switch (size)
            {
                case IconSize.Large:
                    memoryCache = LargeIconCache;
                    break;
                case IconSize.Small:
                    memoryCache = SmallIconCache;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(size)} is not a valid icon size");
            }
            try
            {
                // todo: IconCache.CacheMemoryLimit; setting
                var cacheItem = memoryCache.GetCacheItem(filePath);
                if (cacheItem != null)
                    return cacheItem.Value as ImageSource;
                var shinfo = new NativeMethods.ShFileInfo();
                var hImgSmall = NativeMethods.SHGetFileInfo(filePath, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                    NativeMethods.SHGFI_ICON | size.ToIconFlag());
                var icon = Icon.FromHandle(shinfo.hIcon);
                var bmp = icon.ToBitmap();
                var img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                img.Freeze();
                var newItem = new CacheItem(filePath, img);
                var policy = new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };
                memoryCache.Add(newItem, policy);
                return img;
            }
            catch (Exception)
            {
                if (!shouldFallBack)
                    return null;
                switch (size)
                {
                    case IconSize.Large:
                        return UnknownFileLarge;
                    case IconSize.Small:
                        return UnknownFileSmall;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(size)} is not a valid IconSize");
                }
            }
        }
    }
}