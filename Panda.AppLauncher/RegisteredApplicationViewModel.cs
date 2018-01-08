﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Panda.Client;
using Panda.Client.Properties;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for registered applications
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class RegisteredApplicationViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The image source for the icon
        /// </summary>
        private ImageSource _imageSource;

        private bool _isEditable;
        private string _appName;
        private string _executableLocation;
        private RegisteredApplication _registeredApplication;

        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        /// <value>
        ///     The name of the application.
        /// </value>
        public string AppName
        {
            get => _appName;
            set
            {
                OnPropertyChanged();
                _appName = value;
                if(_registeredApplication != null)
                    _registeredApplication.DisplayName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the executable location.
        /// </summary>
        /// <value>
        ///     The executable location.
        /// </value>
        public string ExecutableLocation
        {
            get => _executableLocation;
            set
            {
                OnPropertyChanged();
                _executableLocation = value;
                if(_registeredApplication != null)
                    _registeredApplication.FullPath = value;
            }
        }

        /// <summary>
        ///     Gets or sets the registered application.
        /// </summary>
        /// <value>
        ///     The registered application.
        /// </value>
        public RegisteredApplication RegisteredApplication
        {
            get => _registeredApplication;
            set
            {
                OnPropertyChanged();
                _registeredApplication = value;
                AppName = value?.DisplayName;
                ExecutableLocation = value?.FullPath;
            }
        }

        /// <summary>
        ///     Gets or sets the image source.
        /// </summary>
        /// <value>
        ///     The image source.
        /// </value>
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Loads the icon.
        /// </summary>
        /// <returns>A task, that when complete, will signal the completion of the loading of the image source</returns>
        public Task LoadIcon(IconSize iconSize)
        {
            return Task.Run(() => { ImageSource = IconHelper.IconFromFilePath(ExecutableLocation, iconSize); });
        }

        public RegisteredApplicationViewModel(IRegisteredApplicationService launcherService)
        {
            RegisteredApplicationService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
            AddEditMenuItem();
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value; 
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FrameworkElement> MenuItems { get; set; } = new ObservableCollection<FrameworkElement>();
                                
        private void AddEditMenuItem()
        {
            var menuItem = new MenuItem { Header = "Edit" };
            menuItem.Click += (sender, args) =>
            {
                IsEditable = true;
                MenuItems.Remove(menuItem);
                AddSaveMenuItem();
            };
            MenuItems.Add(menuItem);
        }
      
        public IRegisteredApplicationService RegisteredApplicationService { get; set; }

        private void AddSaveMenuItem()
        {
            var menuItem = new MenuItem { Header = "Save" };
            menuItem.Click += (sender, args) =>
            {
                IsEditable = false;
                MenuItems.Remove(menuItem);
                AddEditMenuItem();    
                RegisteredApplicationService.Save();
            };
            MenuItems.Add(menuItem);
        }
    }
}