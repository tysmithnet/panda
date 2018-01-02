using System;
using System.Windows.Forms;

namespace Panda.Client
{
    /// <summary>
    ///     A service that will provide functionality related to receiving system wide keyboard and mouse events
    /// </summary>
    public interface IKeyboardMouseHookService
    {
        /// <summary>
        ///     Gets an observable that will deliver events when a KeyDown event occurs
        /// </summary>
        /// <value>
        ///     The key down observable.
        /// </value>
        IObservable<KeyEventArgs> KeyDownObservable { get; }

        /// <summary>
        ///     Gets an observable that will deliver events when a KeyPress event occurs
        /// </summary>
        /// <value>
        ///     The key press observable.
        /// </value>
        IObservable<KeyPressEventArgs> KeyPressObservable { get; }

        /// <summary>
        ///     Gets an observable that will deliver events when a KeyUp event occurs
        /// </summary>
        /// <value>
        ///     The key up observable.
        /// </value>
        IObservable<KeyEventArgs> KeyUpObservable { get; }
    }
}