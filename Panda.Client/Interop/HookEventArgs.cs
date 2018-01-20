using System;

namespace Panda.Client.Interop
{
    public class HookEventArgs : EventArgs
    {
        public int HookCode; // Hook code
        public IntPtr lParam; // LPARAM argument
        public IntPtr wParam; // WPARAM argument
    }
}