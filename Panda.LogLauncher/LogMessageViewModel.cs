using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panda.LogLauncher.Annotations;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Class LogMessageViewModel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class LogMessageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the log time.
        /// </summary>
        /// <value>The log time.</value>
        public DateTime LogTime { get; set; }

        /// <summary>
        ///     Gets or sets the name of the log.
        /// </summary>
        /// <value>The name of the log.</value>
        public string LogName { get; set; }

        /// <summary>
        ///     Gets or sets the log level.
        /// </summary>
        /// <value>The log level.</value>
        public string LogLevel { get; set; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public string Exception { get; set; }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}