﻿#region Copyright
// <copyright file="StateManager.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// SmartThings.NETCoreWebHookSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SmartThings.NETCoreWebHookSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
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
        ILogger<IStateManager<T>> logger { get; }
        IList<IObserver<string>> observers { get; }
        IDictionary<string, T> stateCache { get; }
        Task<T> GetStateAsync(string installedAppId);
        Task StoreStateAsync(string installedAppId, T state);
        Task RemoveStateAsync(string installedAppId);
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

        public virtual async Task<T> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogDebug($"Getting state from cache: {installedAppId}...");

            T state = default(T);
            if (stateCache.TryGetValue(installedAppId, out state))
            {
                logger.LogDebug($"Got state from cache: {installedAppId}...");
            }
            else
            {
                logger.LogDebug($"Unable to find state in cache: {installedAppId}...");
            }

            return state;
        }

        public virtual async Task StoreStateAsync(string installedAppId, T state)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogDebug($"Adding state to cache: {installedAppId}...");

            stateCache.Remove(installedAppId);
            stateCache.Add(installedAppId, state);

            NotifyObservers(installedAppId);

            await PersistCacheAsync();
        }

        public virtual async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogDebug($"Removing state from cache: {installedAppId}...");

            stateCache.Remove(installedAppId);

            NotifyObservers(installedAppId);

            await PersistCacheAsync();
        }

        public abstract Task PersistCacheAsync();

        public abstract Task LoadCacheAsync();

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

        private void NotifyObservers(string installedAppId)
        {
            logger.LogDebug($"Notifying observers of state change for: {installedAppId}");

            foreach (var observer in observers)
            {
                observer.OnNext(installedAppId);
            }
        }
    }
}
