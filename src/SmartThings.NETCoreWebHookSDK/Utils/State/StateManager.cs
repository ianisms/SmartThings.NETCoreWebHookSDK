#region Copyright
// <copyright file="StateManager.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
#endregion
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public interface IStateManager<T> : IObservable<string>
    {
        Task LoadCacheAsync();
        Task<T> GetStateAsync(string installedAppId);
        Task StoreStateAsync(string installedAppId, T state);
        Task RemoveStateAsync(string installedAppId);
    }

    public abstract class StateManager<T> : IStateManager<T>
    {
        public IDictionary<string, T> StateCache { get; set; }

        private readonly ILogger<IStateManager<T>> _logger;
        private readonly IList<IObserver<string>> _observers;

        public StateManager(ILogger<IStateManager<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _observers = new List<IObserver<string>>();
        }

        public virtual async Task<T> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync().ConfigureAwait(false);

            _logger.LogDebug($"Getting state from cache: {installedAppId}...");

            if (StateCache.TryGetValue(installedAppId, out T state))
            {
                _logger.LogDebug($"Got state from cache: {installedAppId}...");
            }
            else
            {
                _logger.LogDebug($"Unable to find state in cache: {installedAppId}...");
            }

            return state;
        }

        public virtual async Task StoreStateAsync(string installedAppId, T state)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync().ConfigureAwait(false);

            _logger.LogDebug($"Adding state to cache: {installedAppId}...");

            StateCache.Remove(installedAppId);
            StateCache.Add(installedAppId, state);

            NotifyObservers(installedAppId);

            await PersistCacheAsync().ConfigureAwait(false);
        }

        public virtual async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync().ConfigureAwait(false);

            _logger.LogDebug($"Removing state from cache: {installedAppId}...");

            StateCache.Remove(installedAppId);

            NotifyObservers(installedAppId);

            await PersistCacheAsync().ConfigureAwait(false);
        }

        public abstract Task PersistCacheAsync();

        public abstract Task LoadCacheAsync();

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber(_observers, observer);
        }

        public void UnSubscribe(IObserver<string> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly IList<IObserver<string>> observers;
            private readonly IObserver<string> observer;

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

        private void NotifyObservers(string installedAppId)
        {
            _logger.LogDebug($"Notifying observers of state change for: {installedAppId}");

            foreach (var observer in _observers)
            {
                observer.OnNext(installedAppId);
            }
        }
    }
}
