using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Panda.Client
{
    internal class NativeMethods
    {
        /// <summary>
        ///     Win32 Api Shell File Info structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ShFileInfo
        {
            public readonly IntPtr hIcon;
            private readonly int iIcon;
            private readonly uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public readonly string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public readonly string szTypeName;
        }

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi,
            uint cbSizeFileInfo, uint uFlags);

        [DllImport("shell32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        internal static extern int SHGetLocalizedName(string pszPath, StringBuilder pszResModule, ref int cch, out int pidsRes);
    }
}