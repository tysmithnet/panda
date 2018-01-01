using System;
using System.Windows.Forms;

namespace Panda.Client
{
    public interface IKeyboardMouseHookService
    {
        IObservable<KeyEventArgs> KeyDownObservable { get; }
        IObservable<KeyPressEventArgs> KeyPressObservable { get; }
        IObservable<KeyEventArgs> KeyUpObservable { get; }
    }
}