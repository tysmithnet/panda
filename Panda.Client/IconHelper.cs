﻿using System;
using System.Drawing;
using System.Runtime.Caching;
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
        ///     The large icon cache
        /// </summary>
        private static readonly MemoryCache LargeIconCache = new MemoryCache(typeof(IconHelper).FullName + "_large");

        /// <summary>
        ///     The small icon cache
        /// </summary>
        private static readonly MemoryCache SmallIconCache = new MemoryCache(typeof(IconHelper).FullName + "_small");

        /// <summary>
        ///     The unknown file large
        /// </summary>
        private static readonly ImageSource UnknownFileLarge;

        /// <summary>
        ///     The unknown file small
        /// </summary>
        private static readonly ImageSource UnknownFileSmall;

        /// <summary>
        ///     Initializes the <see cref="IconHelper" /> class.
        /// </summary>
        static IconHelper()
        {
            var shinfo = new ShFileInfo();
            var hImgSmall = Win32.SHGetFileInfo("unknownfile32x32.ico", 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                Win32.SHGFI_ICON | IconSize.Large.ToIconFlag());
            var icon = Icon.FromHandle(shinfo.hIcon);
            var bmp = icon.ToBitmap();
            var img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            UnknownFileLarge = img;

            hImgSmall = Win32.SHGetFileInfo("unknownfile16x16.ico", 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                Win32.SHGFI_ICON | IconSize.Small.ToIconFlag());
            icon = Icon.FromHandle(shinfo.hIcon);
            bmp = icon.ToBitmap();
            img = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            UnknownFileSmall = img;
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
                var shinfo = new ShFileInfo();
                var hImgSmall = Win32.SHGetFileInfo(filePath, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                    Win32.SHGFI_ICON | size.ToIconFlag());
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

        /// <summary>
        ///     Win32 Api Shell File Info structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ShFileInfo
        {
            public readonly IntPtr hIcon;
            readonly int iIcon;
            readonly uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            readonly string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            readonly string szTypeName;
        }

        /// <summary>
        ///     Local win32 api wrapper
        /// </summary>
        private static class Win32
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