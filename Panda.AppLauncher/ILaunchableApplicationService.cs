using System;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     A service that is capable of managing the RegisteredApplication domain
    /// </summary>
    public interface ILaunchableApplicationService
    {
        /// <summary>
        ///     Gets an observable that will deliver newly registered applications
        /// </summary>
        /// <value>
        ///     The application registered observable.
        /// </value>
        IObservable<LaunchableApplication> ApplicationRegisteredObservable { get; }

        /// <summary>
        ///     Gets an observable that will delivery notification of removed applications
        /// </summary>
        /// <value>
        ///     The application unregistered observable.
        /// </value>
        IObservable<LaunchableApplication> ApplicationUnregisteredObservable { get; }

        /// <summary>
        ///     Registers a new application
        /// </summary>
        /// <param name="launchableApplication">The registered application to register.</param>
        void Add(LaunchableApplication launchableApplication);

        /// <summary>
        ///     Gets an observable that will deliver all registered applications when called
        /// </summary>
        /// <returns>An observable that will deliver all currently registered applications</returns>
        IObservable<LaunchableApplication> Get();

        /// <summary>
        ///     Unregisters an application
        /// </summary>
        /// <param name="launchableApplication">The registered application to remove</param>
        void Remove(LaunchableApplication launchableApplication);

        /// <summary>
        ///     Saves the current state of registered applications
        /// </summary>
        void Save();
    }
}