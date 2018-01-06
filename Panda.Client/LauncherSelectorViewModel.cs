using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace Panda.Client
{
    /// <summary>
    ///     View model for the launcher selector
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class LauncherSelectorViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The preview mouse up obs
        /// </summary>
        private IObservable<(LauncherViewModel, MouseButtonEventArgs)> _previewMouseUpObs;

        /// <summary>
        /// The preview mouse up subscription
        /// </summary>
        private IDisposable _previewMouseUpSubscription;

        /// <summary>
        /// The search text box preview key up obs
        /// </summary>
        private IObservable<(string, KeyEventArgs)> _searchTextBoxPreviewKeyUpObs;

        /// <summary>
        /// The search text box preview key up subscription
        /// </summary>
        private IDisposable _searchTextBoxPreviewKeyUpSubscription;

        /// <summary>
        ///     The selection changed obs
        /// </summary>
        private IObservable<SelectionChangedEventArgs> _selectionChangedObs;

        /// <summary>
        ///     The selection changed subscription
        /// </summary>
        private IDisposable _selectionChangedSubscription;

        /// <summary>
        ///     The text changed obs
        /// </summary>
        private IObservable<string> _textChangedObs;

        /// <summary>
        ///     The text changed subscription
        /// </summary>
        private IDisposable _textChangedSubscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LauncherSelectorViewModel" /> class.
        /// </summary>
        /// <param name="launcherService">The launcher service.</param>
        public LauncherSelectorViewModel(ILauncherService launcherService)
        {
            LauncherService = launcherService;
            ViewModels = LauncherService.Get().Select(l => new LauncherViewModel
            {
                Name = l.GetType().FullName,
                Instance = l
            });
            LauncherViewModels = new ObservableCollection<LauncherViewModel>(ViewModels);
        }

        /// <summary>
        ///     Gets or sets the text changed obs.
        /// </summary>
        /// <value>
        ///     The text changed obs.
        /// </value>
        public IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                _textChangedSubscription?.Dispose();
                _textChangedObs = value;
                _textChangedSubscription = value
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(filter =>
                    {
                        LauncherViewModels.Clear();

                        if (string.IsNullOrEmpty(filter))
                        {
                            foreach (var launcherViewModel in ViewModels)
                                LauncherViewModels.Add(launcherViewModel);
                            return;
                        }

                        foreach (var launcherViewModel in ViewModels.Where(vm =>
                            Regex.IsMatch(vm.Name, filter, RegexOptions.IgnoreCase)))
                            LauncherViewModels.Add(launcherViewModel);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the launcher service.
        /// </summary>
        /// <value>
        ///     The launcher service.
        /// </value>
        internal ILauncherService LauncherService { get; set; }

        /// <summary>
        ///     Gets or sets the view models.
        /// </summary>
        /// <value>
        ///     The view models.
        /// </value>
        internal IEnumerable<LauncherViewModel> ViewModels { get; set; }

        /// <summary>
        ///     Gets or sets the active.
        /// </summary>
        /// <value>
        ///     The active.
        /// </value>
        internal Launcher Active { get; set; }

        /// <summary>
        ///     Gets or sets the launcher view models.
        /// </summary>
        /// <value>
        ///     The launcher view models.
        /// </value>
        public ObservableCollection<LauncherViewModel> LauncherViewModels { get; set; }

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>
        ///     The search text.
        /// </value>
        public string SearchText { get; set; }

        /// <summary>
        ///     Gets or sets the selection changed obs.
        /// </summary>
        /// <value>
        ///     The selection changed obs.
        /// </value>
        public IObservable<SelectionChangedEventArgs> SelectionChangedObs
        {
            get => _selectionChangedObs;
            set
            {
                _selectionChangedSubscription?.Dispose();
                _selectionChangedObs = value;
                _selectionChangedSubscription = value.Subscribe(e =>
                {
                    var added = e.AddedItems.Cast<LauncherViewModel>();
                    var launcherViewModels = added as LauncherViewModel[] ?? added.ToArray();
                    if (launcherViewModels.Any())
                    {
                        var first = launcherViewModels.First();
                        Active?.Hide();
                        Active = first.Instance;
                        Active.Show();
                    }
                });
            }
        }

        /// <summary>
        /// Gets or sets the preview mouse up obs.
        /// </summary>
        /// <value>
        /// The preview mouse up obs.
        /// </value>
        public IObservable<(LauncherViewModel, MouseButtonEventArgs)> PreviewMouseUpObs
        {
            get => _previewMouseUpObs;
            set
            {
                _previewMouseUpSubscription?.Dispose();
                _previewMouseUpObs = value;
                _previewMouseUpSubscription = value.Subscribe(tuple =>
                {
                    Active?.Hide();
                    Active = tuple.Item1.Instance;
                    Active.Show();
                });
            }
        }

        /// <summary>
        /// Gets or sets the search text box preview key up obs.
        /// </summary>
        /// <value>
        /// The search text box preview key up obs.
        /// </value>
        public IObservable<(string, KeyEventArgs)> SearchTextBoxPreviewKeyUpObs
        {
            get => _searchTextBoxPreviewKeyUpObs;
            set
            {
                _searchTextBoxPreviewKeyUpSubscription?.Dispose();
                _searchTextBoxPreviewKeyUpObs = value;
                _searchTextBoxPreviewKeyUpSubscription = value.Subscribe(tuple =>
                {
                    var currentText = tuple.Item1;
                    var args = tuple.Item2;

                    if (new[] {Key.Enter, Key.Return}.Contains(args.Key))
                        StartFirst();
                });
            }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Submits this instance.
        /// </summary>
        public void StartFirst()
        {
            var first = LauncherViewModels.FirstOrDefault();
            if (first == null) return;
            Active?.Hide();
            Active = first.Instance;
            Active.Show();
            // todo: allow multiple active windows
        }
    }
}