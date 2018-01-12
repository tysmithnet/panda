using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
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
        ///     The launcher selector key up obs
        /// </summary>
        private IObservable<KeyEventArgs> _launcherSelectorKeyUpObs;

        /// <summary>
        ///     The launcher selector key up subscription
        /// </summary>
        private IDisposable _launcherSelectorKeyUpSubscription;

        /// <summary>
        ///     The preview mouse up obs
        /// </summary>
        private IObservable<(LauncherViewModel, MouseButtonEventArgs)> _mouseUpObs;

        /// <summary>
        ///     The preview mouse up subscription
        /// </summary>
        private IDisposable _previewMouseUpSubscription;

        /// <summary>
        ///     The search text box preview key up obs
        /// </summary>
        private IObservable<(string, KeyEventArgs)> _searchTextBoxPreviewKeyUpObs;

        /// <summary>
        ///     The search text box preview key up subscription
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

        private string _searchText;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LauncherSelectorViewModel" /> class.
        /// </summary>
        /// <param name="launcherService">The launcher service.</param>
        internal LauncherSelectorViewModel(ILauncherService launcherService)
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
        internal IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                _textChangedSubscription?.Dispose();
                _textChangedObs = value;
                _textChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
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
        private ILauncherService LauncherService { get; }

        /// <summary>
        ///     Gets or sets the view models.
        /// </summary>
        /// <value>
        ///     The view models.
        /// </value>
        private IEnumerable<LauncherViewModel> ViewModels { get; }

        /// <summary>
        ///     Gets or sets the active.
        /// </summary>
        /// <value>
        ///     The active.
        /// </value>
        private Launcher Active { get; set; }

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
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value; 
                OnPropertyChanged();
            }
        }

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
                        SearchText = "";
                    }
                });
            }
        }

        /// <summary>
        ///     Gets or sets the preview mouse up obs.
        /// </summary>
        /// <value>
        ///     The preview mouse up obs.
        /// </value>
        internal IObservable<(LauncherViewModel, MouseButtonEventArgs)> MouseUpObs
        {
            get => _mouseUpObs;
            set
            {
                _previewMouseUpSubscription?.Dispose();
                _mouseUpObs = value;
                _previewMouseUpSubscription = value.Subscribe(tuple =>
                {
                    Active?.Hide();
                    Active = tuple.Item1.Instance;
                    Active.Show();
                    SearchText = "";
                });
            }
        }

        /// <summary>
        ///     Gets or sets the search text box preview key up obs.
        /// </summary>
        /// <value>
        ///     The search text box preview key up obs.
        /// </value>
        internal IObservable<(string, KeyEventArgs)> SearchTextBoxPreviewKeyUpObs
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
                    {
                        StartFirst();
                        args.Handled = true;
                    }
                });
            }
        }

        /// <summary>
        ///     Gets or sets the hide action.
        /// </summary>
        /// <value>
        ///     The hide action.
        /// </value>
        internal Action HideAction { get; set; }

        /// <summary>
        ///     Gets or sets the launcher selector key up obs.
        /// </summary>
        /// <value>
        ///     The launcher selector key up obs.
        /// </value>
        internal IObservable<KeyEventArgs> LauncherSelectorKeyUpObs
        {
            get => _launcherSelectorKeyUpObs;
            set
            {
                _launcherSelectorKeyUpSubscription?.Dispose();
                _launcherSelectorKeyUpObs = value;
                _launcherSelectorKeyUpSubscription = value.Subscribe(args =>
                {
                    if (args.Key == Key.Escape)
                    {
                        HideAction?.Invoke();
                        args.Handled = true;
                    }
                });
            }
        }

        public IScheduler UiScheduler { get; set; }

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
        private void StartFirst()
        {
            var first = LauncherViewModels.FirstOrDefault();
            if (first == null) return;
            Active?.Hide();
            Active = first.Instance;
            Active.Show();
            SearchText = "";
            // todo: allow multiple active windows
        }
    }
}