﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IRequiresSetup))]
    [Export(typeof(RegisteredApplicationRepository))]
    public class RegisteredApplicationRepository : IRequiresSetup
    {
        protected internal Subject<RegisteredApplication> ApplicationRegisteredSubject =
            new Subject<RegisteredApplication>();

        protected internal Subject<RegisteredApplication> ApplicationUnregisteredSubject =
            new Subject<RegisteredApplication>();

        protected internal IList<RegisteredApplication> RegisteredApplications { get; set; } =
            new List<RegisteredApplication>();

        [Import]
        protected internal SettingsService SettingsService { get; set; }

        public IObservable<RegisteredApplication> ApplicationRegisteredObservable => ApplicationRegisteredSubject;

        public IObservable<RegisteredApplication> ApplicationUnregisteredObservable => ApplicationUnregisteredSubject;

        public AppLauncherSettings Settings { get; set; }

        public Task Setup(CancellationToken cancellationToken)
        {
            Settings = SettingsService.Get<AppLauncherSettings>().Single();
            foreach (var registeredApplication in Settings.RegisteredApplications)
            {
                var revivedApp = new RegisteredApplication
                {
                    DisplayName = registeredApplication.DisplayName,
                    FullPath = registeredApplication.FullPath
                };
                RegisteredApplications.Add(revivedApp);
            }
            return Task.CompletedTask;
        }

        public IEnumerable<RegisteredApplication> Get()
        {
            return RegisteredApplications;
        }

        public void Add(RegisteredApplication registeredApplication)
        {   
            Settings.RegisteredApplications.Add(registeredApplication);
            RegisteredApplications.Add(registeredApplication);
            ApplicationRegisteredSubject.OnNext(registeredApplication);
        }

        public void Remove(RegisteredApplication registeredApplication)
        {
            Settings.RegisteredApplications.Remove(registeredApplication); // todo: test
            RegisteredApplications.Remove(registeredApplication);
            ApplicationUnregisteredSubject.OnNext(registeredApplication);
        }

        public void Save()
        {
            SettingsService.Save(); // todo: fix
        }
    }
}