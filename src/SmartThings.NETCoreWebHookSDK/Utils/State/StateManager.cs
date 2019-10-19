using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public interface IStateManager<T> : IObservable<string>
    {
        ILogger<IStateManager<T>> logger { get; }
        IList<IObserver<string>> observers { get; }
        IDictionary<string, T> stateCache { get; }
        Task<T> GetStateAsync(string installedAppId);
        Task StoreStateAsync(string installedAppId, T state);
        Task StoreStateAndNotifyAsync(string installedAppId, T state);
        Task RemoveStateAsync(string installedAppId);
        Task RemoveStateAndNotifyAsync(string installedAppId);
    }

    public abstract class StateManager<T> : IStateManager<T>
    {
        public ILogger<IStateManager<T>> logger { get; private set; }
        public IList<IObserver<string>> observers { get; private set; }
        public IDictionary<string, T> stateCache { get; set; }

        public StateManager(ILogger<IStateManager<T>> logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            this.logger = logger;
            this.observers = new List<IObserver<string>>();
        }

        private void NotifyObservers(string installedAppId)
        {
            logger.LogInformation($"Notifying observers of state change for: {installedAppId}");

            foreach (var observer in observers)
            {
                observer.OnNext(installedAppId);
            }
        }

        public virtual async Task<T> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Getting state from cache: {installedAppId}...");

            T state = default(T);
            if (stateCache.TryGetValue(installedAppId, out state))
            {
                logger.LogDebug($"Got state from cache: {installedAppId}...");
            }
            else
            {
                logger.LogInformation($"Unable to find state in cache: {installedAppId}...");
            }

            return state;
        }

        public virtual async Task StoreStateAsync(string installedAppId, T state)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Adding state to cache: {installedAppId}...");

            stateCache.Remove(installedAppId);
            stateCache.Add(installedAppId, state);

            await PersistCacheAsync();
        }

        public virtual async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Removing state from cache: {installedAppId}...");

            stateCache.Remove(installedAppId);

            await PersistCacheAsync();
        }

        public abstract Task PersistCacheAsync();

        public abstract Task LoadCacheAsync();

        public virtual async Task StoreStateAndNotifyAsync(string installedAppId, T state)
        {
            await StoreStateAsync(installedAppId, state);
            NotifyObservers(installedAppId);
        }

        public virtual async Task RemoveStateAndNotifyAsync(string installedAppId)
        {
            await RemoveStateAsync(installedAppId);
            NotifyObservers(installedAppId);
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if(!observers.Contains(observer))
            {
                observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }

        public void UnSubscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Remove(observer);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private IList<IObserver<string>> observers;
            private IObserver<string> observer;

            public Unsubscriber(IList<IObserver<string>> observers, IObserver<string> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose()
            {
                if (observer != null && observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
        }
    }
}
