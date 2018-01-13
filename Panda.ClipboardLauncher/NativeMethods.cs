using System;
using System.Runtime.InteropServices;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Native methods for interacting with the clipboard
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("User32.dll")]
        internal static extern int
            SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool
            ChangeClipboardChain(IntPtr hWndRemove,
                IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(IntPtr hwnd, int wMsg,
            IntPtr wParam,
            IntPtr lParam);
    }
}