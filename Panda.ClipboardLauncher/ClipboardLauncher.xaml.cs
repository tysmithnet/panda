﻿using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    /// <seealso cref="Panda.Client.Launcher" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [Export(typeof(Launcher))]
    public sealed partial class ClipboardLauncher : Launcher
    {
        /// <summary>
        ///     The search text changed
        /// </summary>
        private readonly Subject<string> _searchTextChanged = new Subject<string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardLauncher" /> class.
        /// </summary>
        public ClipboardLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        private ClipboardLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Handles the OnLoaded event of the ClipboardLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void ClipboardLauncher_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchText.Focus();
            ViewModel = new ClipboardLauncherViewModel(this, UiScheduler, SettingsService)
            {
                SearchTextChangedObs = _searchTextChanged
            };
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the SearchText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void SearchText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTextChanged.OnNext(SearchText.Text);
        }
    }
}