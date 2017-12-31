using System;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Panda.Client
{
    [Export]
    public class KeyboardMouseHookService
    {
        protected internal Subject<KeyEventArgs> KeyDownSubject = new Subject<KeyEventArgs>();
        protected internal Subject<KeyPressEventArgs> KeyPressSubject = new Subject<KeyPressEventArgs>();
        protected internal Subject<KeyEventArgs> KeyUpSubject = new Subject<KeyEventArgs>();


        public KeyboardMouseHookService()
        {
            GlobalEvents = Hook.GlobalEvents();
            AppEvents = Hook.AppEvents();
            GlobalEvents.KeyDown += (sender, args) => KeyDownSubject.OnNext(args);
            GlobalEvents.KeyPress += (sender, args) => KeyPressSubject.OnNext(args);
            GlobalEvents.KeyUp += (sender, args) => KeyUpSubject.OnNext(args);
        }


        public IObservable<KeyEventArgs> KeyDownObservable => KeyDownSubject;
        public IObservable<KeyPressEventArgs> KeyPressObservable => KeyPressSubject;
        public IObservable<KeyEventArgs> KeyUpObservable => KeyUpSubject;

        public IKeyboardMouseEvents AppEvents { get; set; }

        public IKeyboardMouseEvents GlobalEvents { get; set; }
    }
}