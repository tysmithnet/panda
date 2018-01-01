using System;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Panda.Client
{
    [Export(typeof(IKeyboardMouseHookService))]
    public sealed class KeyboardMouseHookService : IKeyboardMouseHookService
    {
        internal Subject<KeyEventArgs> KeyDownSubject = new Subject<KeyEventArgs>();
        internal Subject<KeyPressEventArgs> KeyPressSubject = new Subject<KeyPressEventArgs>();
        internal Subject<KeyEventArgs> KeyUpSubject = new Subject<KeyEventArgs>();


        internal KeyboardMouseHookService()
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
    }
}