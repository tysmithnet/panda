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
    /// <inheritdoc />
    /// <summary>
    ///     Default implementation of IKeyboardMouseService
    /// </summary>
    /// <seealso cref="T:Panda.Client.IKeyboardMouseService" />
    [Export(typeof(IKeyboardMouseService))]
    public sealed class KeyboardMouseService : IKeyboardMouseService
    {
        /// <summary>
        ///     The key down subject
        /// </summary>
        internal Subject<KeyEventArgs> KeyDownSubject = new Subject<KeyEventArgs>();

        /// <summary>
        ///     The key press subject
        /// </summary>
        internal Subject<KeyPressEventArgs> KeyPressSubject = new Subject<KeyPressEventArgs>();

        /// <summary>
        ///     The key up subject
        /// </summary>
        internal Subject<KeyEventArgs> KeyUpSubject = new Subject<KeyEventArgs>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyboardMouseService" /> class.
        /// </summary>
        internal KeyboardMouseService()
        {
            GlobalEvents = Hook.GlobalEvents();
            AppEvents = Hook.AppEvents();
            GlobalEvents.KeyDown += (sender, args) => KeyDownSubject.OnNext(args);
            GlobalEvents.KeyPress += (sender, args) => KeyPressSubject.OnNext(args);
            GlobalEvents.KeyUp += (sender, args) => KeyUpSubject.OnNext(args);
        }

        /// <summary>
        ///     Gets or sets the application event source.
        /// </summary>
        /// <value>
        ///     The application event source.
        /// </value>
        internal IKeyboardMouseEvents AppEvents { get; set; }

        /// <summary>
        ///     Gets or sets the global event source.
        /// </summary>
        /// <value>
        ///     The global event source.
        /// </value>
        internal IKeyboardMouseEvents GlobalEvents { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver events when a KeyDown event occurs
        /// </summary>
        /// <value>
        ///     The key down observable.
        /// </value>
        public IObservable<KeyEventArgs> KeyDownObservable => KeyDownSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver events when a KeyPress event occurs
        /// </summary>
        /// <value>
        ///     The key press observable.
        /// </value>
        public IObservable<KeyPressEventArgs> KeyPressObservable => KeyPressSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver events when a KeyUp event occurs
        /// </summary>
        /// <value>
        ///     The key up observable.
        /// </value>
        public IObservable<KeyEventArgs> KeyUpObservable => KeyUpSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether the specified key is currently down
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     <c>true</c> if the specified key is down, otherwise, <c>false</c>.
        /// </returns>
        public bool IsKeyDown(Key key)
        {
            return Keyboard.IsKeyDown(key);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the current mouse position
        /// </summary>
        /// <returns>
        ///     The current mouse position
        /// </returns>
        public Point GetMousePosition()
        {
            GetCursorPos(out var pointStruct);
            return pointStruct;
        }

        /// <summary>
        ///     Gets the cursor position.
        /// </summary>
        /// <param name="lpPoint">The lp point.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out PointStruct lpPoint);

        /// <summary>
        ///     Represents a pair of mouse coordinates
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PointStruct
        {
            public int X;
            public int Y;

            /// <summary>
            ///     Performs an implicit conversion from <see cref="PointStruct" /> to <see cref="Point" />.
            /// </summary>
            /// <param name="pointStruct">The point structure.</param>
            /// <returns>
            ///     The result of the conversion.
            /// </returns>
            public static implicit operator Point(PointStruct pointStruct)
            {
                return new Point(pointStruct.X, pointStruct.Y);
            }
        }
    }
}