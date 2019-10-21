#region Copyright
// <copyright file="InstalledAppManager.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public interface IInstalledAppManager : IObservable<string>
    {
        ILogger<IInstalledAppManager> Logger { get; }
        ISmartThingsAPIHelper SmartThingsAPIHelper { get; }
        IDictionary<string, Models.SmartThings.InstalledApp> InstalledAppCache { get; }
        IList<IObserver<string>> observers { get; }
        Task<Models.SmartThings.InstalledApp> GetInstalledAppAsync(string installedAppId);
        Task StoreInstalledAppAsync(Models.SmartThings.InstalledApp installedApp);
        Task RemoveInstalledAppAsync(string installedAppId);
        Task PersistCacheAsync();
        Task LoadCacheAsync();
        Task<Models.SmartThings.InstalledApp> RefreshTokensAsync(Models.SmartThings.InstalledApp installedApp);
        Task RefreshAllInstalledAppTokensAsync();
    }

    public abstract class InstalledAppManager : IInstalledAppManager
    {
        public ILogger<IInstalledAppManager> Logger { get; private set; }
        public ISmartThingsAPIHelper SmartThingsAPIHelper { get; private set; }
        public IDictionary<string, Models.SmartThings.InstalledApp> InstalledAppCache { get; set; }
        public IList<IObserver<string>> observers { get; private set; }

        public InstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = smartThingsAPIHelper ??
                throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.Logger = logger;
            this.SmartThingsAPIHelper = smartThingsAPIHelper;
            this.observers = new List<IObserver<string>>();
        }

        public virtual async Task<Models.SmartThings.InstalledApp> GetInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            Logger.LogDebug($"Getting installedApp from cache: {installedAppId}...");

            Models.SmartThings.InstalledApp installedApp = null;
            if (InstalledAppCache.TryGetValue(installedAppId, out installedApp))
            {
                Logger.LogDebug($"Got installedApp from cache: {installedAppId}...");
            }
            else
            {
                Logger.LogDebug($"Unable to find installedApp in cache: {installedAppId}...");
            }

            return installedApp;
        }

        public virtual async Task StoreInstalledAppAsync(Models.SmartThings.InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException($"installedApp.InstalledAppId is null",
                nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException($"installedApp.AccessToken is null",
                nameof(installedApp));

            await LoadCacheAsync();

            Logger.LogDebug($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

            InstalledAppCache.Remove(installedApp.InstalledAppId);
            InstalledAppCache.Add(installedApp.InstalledAppId, installedApp);
            NotifyObservers(installedApp.InstalledAppId);

            await PersistCacheAsync();
        }

        public virtual async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            Logger.LogDebug($"Removing installedApp from cache: {installedAppId}...");

            InstalledAppCache.Remove(installedAppId);
            NotifyObservers(installedAppId);

            await PersistCacheAsync();
        }

        public abstract Task PersistCacheAsync();

        public abstract Task LoadCacheAsync();

        public virtual async Task<Models.SmartThings.InstalledApp> RefreshTokensAsync(Models.SmartThings.InstalledApp installedApp)
        {
            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException("installedApp.InstalledAppId is null", nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException("installedApp.AccessToken is null", nameof(installedApp));
            _ = installedApp.RefreshToken ??
                throw new ArgumentException("installedApp.RefreshToken is null", nameof(installedApp));
            if (installedApp.RefreshToken.IsExpired)
            {
                throw new ArgumentException("installedApp.RefreshToken is expired!", nameof(installedApp));
            }

            Logger.LogDebug($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            if (installedApp.AccessToken.IsExpired)
            {
                Logger.LogDebug($"installedApp: {installedApp.InstalledAppId} has an expired AuthToken, attempting to refresh...");
                installedApp = await SmartThingsAPIHelper.RefreshTokensAsync(installedApp);
                await StoreInstalledAppAsync(installedApp);
            }

            return installedApp;
        }

        public async Task RefreshAllInstalledAppTokensAsync()
        {
            await LoadCacheAsync();
            var refreshTasks = new List<Task>();
            foreach (var installedApp in InstalledAppCache.Values)
            {
                refreshTasks.Add(RefreshTokensAsync(installedApp));
            }
            Task.WaitAll(refreshTasks.ToArray());
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
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
            Logger.LogDebug($"Notifying observers of state change for: {installedAppId}");

            foreach (var observer in observers)
            {
                observer.OnNext(installedAppId);
            }
        }
    }
}
