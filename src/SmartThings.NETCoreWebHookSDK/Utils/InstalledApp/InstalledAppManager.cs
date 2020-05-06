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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public interface IInstalledAppManager
    {
        ILogger<IInstalledAppManager> Logger { get; }
        ISmartThingsAPIHelper SmartThingsAPIHelper { get; }
        IDictionary<string, InstalledAppInstance> InstalledAppCache { get; }
        Task<InstalledAppInstance> GetInstalledAppAsync(string installedAppId);
        Task StoreInstalledAppAsync(InstalledAppInstance installedApp);
        Task RemoveInstalledAppAsync(string installedAppId);
        Task PersistCacheAsync();
        Task LoadCacheAsync();
        Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp);
        Task RefreshAllInstalledAppTokensAsync();
    }

#pragma warning disable CA1012 // Abstract types should not have constructors
    public abstract class InstalledAppManager : IInstalledAppManager
#pragma warning restore CA1012 // Abstract types should not have constructors
    {
        public ILogger<IInstalledAppManager> Logger { get; private set; }
        public ISmartThingsAPIHelper SmartThingsAPIHelper { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "InstalledAppCache needs to mutable")]
        public IDictionary<string, InstalledAppInstance> InstalledAppCache { get; set; }
        public IList<IObserver<string>> Observers { get; private set; }

        public InstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = smartThingsAPIHelper ??
                throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.Logger = logger;
            this.SmartThingsAPIHelper = smartThingsAPIHelper;
            this.Observers = new List<IObserver<string>>();
        }

        public virtual async Task<InstalledAppInstance> GetInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync().ConfigureAwait(false);

            Logger.LogDebug($"Getting installedApp from cache: {installedAppId}...");

            if (InstalledAppCache.TryGetValue(installedAppId, out InstalledAppInstance installedApp))
            {
                Logger.LogDebug($"Got installedApp from cache: {installedAppId}...");
            }
            else
            {
                Logger.LogDebug($"Unable to find installedApp in cache: {installedAppId}...");
            }

            return installedApp;
        }

        public virtual async Task StoreInstalledAppAsync(InstalledAppInstance installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException($"installedApp.InstalledAppId is null",
                nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException($"installedApp.AccessToken is null",
                nameof(installedApp));

            await LoadCacheAsync().ConfigureAwait(false);

            Logger.LogDebug($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

            InstalledAppCache.Remove(installedApp.InstalledAppId);
            InstalledAppCache.Add(installedApp.InstalledAppId, installedApp);

            await PersistCacheAsync().ConfigureAwait(false);
        }

        public virtual async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            Logger.LogDebug($"Removing installedApp from cache: {installedAppId}...");

            InstalledAppCache.Remove(installedAppId);

            await PersistCacheAsync().ConfigureAwait(false);
        }

        public abstract Task PersistCacheAsync();

        public abstract Task LoadCacheAsync();

        public virtual async Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp)
        {
            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException("installedApp.InstalledAppId is null",
                nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException("installedApp.AccessToken is null",
                nameof(installedApp));
            _ = installedApp.RefreshToken ??
                throw new ArgumentException("installedApp.RefreshToken is null",
                nameof(installedApp));

            if (installedApp.RefreshToken.IsExpired)
            {
                throw new ArgumentException("installedApp.RefreshToken is expired!",
                    nameof(installedApp));
            }

            Logger.LogDebug($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            if (installedApp.AccessToken.IsExpired)
            {
                Logger.LogDebug($"installedApp: {installedApp.InstalledAppId} has an expired token, attempting to refresh...");
                installedApp = await SmartThingsAPIHelper.RefreshTokensAsync(installedApp).ConfigureAwait(false);
                await StoreInstalledAppAsync(installedApp).ConfigureAwait(false);
            }

            return installedApp;
        }

        public async Task RefreshAllInstalledAppTokensAsync()
        {
            await LoadCacheAsync().ConfigureAwait(false);
            var refreshTasks = new List<Task>();
            foreach (var installedApp in InstalledAppCache.Values)
            {
                refreshTasks.Add(RefreshTokensAsync(installedApp));
            }
            Task.WaitAll(refreshTasks.ToArray());
        }
    }
}
