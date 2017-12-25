using System;

namespace Panda.EverythingLauncher.Interop
{
    public class HookEventArgs : EventArgs
    {
        public int HookCode; // Hook code
        public IntPtr lParam; // LPARAM argument
        public IntPtr wParam; // WPARAM argument
    }
}