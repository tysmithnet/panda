﻿using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Input;

namespace Panda.Client
{
    /// <summary>
    ///     Represents a piece of cohesive functionality that can be exposed through the UI
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public class Launcher : Window
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Launcher" /> class.
        /// </summary>
        protected Launcher()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
      
        /// <summary>
        ///     Gets or sets the settings service.
        /// </summary>
        /// <value>
        ///     The settings service.
        /// </value>
        [Import]
        protected ISettingsService SettingsService { get; set; }

        [Import]
        protected IScheduler UiScheduler { get; set; }

        /// <summary>
        ///     Raises the <see cref="E:PreviewKeyDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
                e.Handled = true;
            }
            else
            {
                OnPreviewKeyDown(e);
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs" /> instance containing the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}