using System;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Panda.Client
{
    [Export(typeof(IKeyboardMouseService))]
    public sealed class KeyboardMouseService : IKeyboardMouseService
    {
        internal Subject<KeyEventArgs> KeyDownSubject = new Subject<KeyEventArgs>();
        internal Subject<KeyPressEventArgs> KeyPressSubject = new Subject<KeyPressEventArgs>();
        internal Subject<KeyEventArgs> KeyUpSubject = new Subject<KeyEventArgs>();


        internal KeyboardMouseService()
        {
            GlobalEvents = Hook.GlobalEvents();
            AppEvents = Hook.AppEvents();
            GlobalEvents.KeyDown += (sender, args) => KeyDownSubject.OnNext(args);
            GlobalEvents.KeyPress += (sender, args) => KeyPressSubject.OnNext(args);
            GlobalEvents.KeyUp += (sender, args) => KeyUpSubject.OnNext(args);
        }

        internal IKeyboardMouseEvents AppEvents { get; set; }

        internal IKeyboardMouseEvents GlobalEvents { get; set; }

        public IObservable<KeyEventArgs> KeyDownObservable => KeyDownSubject;
        public IObservable<KeyPressEventArgs> KeyPressObservable => KeyPressSubject;
        public IObservable<KeyEventArgs> KeyUpObservable => KeyUpSubject;
        public bool IsKeyDown(Key key)
        {
            return Keyboard.IsKeyDown(key);
        }

        public Point GetMousePosition()
        {
            GetCursorPos(out var pointStruct);    
            return pointStruct;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PointStruct
        {
            public int X;
            public int Y;

            public static implicit operator Point(PointStruct pointStruct)
            {
                return new Point(pointStruct.X, pointStruct.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out PointStruct lpPoint);
    }
}