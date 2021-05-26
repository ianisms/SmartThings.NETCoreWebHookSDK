#region Copyright
// <copyright file="InstalledAppManager.cs" company="Ian N. Bennett">
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
        Task<InstalledAppInstance> GetInstalledAppAsync(string installedAppId);
        Task StoreInstalledAppAsync(InstalledAppInstance installedApp);
        Task RemoveInstalledAppAsync(string installedAppId);
        Task PersistCacheAsync();
        Task LoadCacheAsync();
        Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp);
        Task RefreshAllInstalledAppTokensAsync();
    }

    public abstract class InstalledAppManager : IInstalledAppManager
    {
        public IDictionary<string, InstalledAppInstance> InstalledAppCache { get; set; }

        private readonly ILogger<IInstalledAppManager> _logger;
        private readonly ISmartThingsAPIHelper _smartThingsAPIHelper;

        public InstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _smartThingsAPIHelper = smartThingsAPIHelper ??
                throw new ArgumentNullException(nameof(smartThingsAPIHelper));
        }

        public virtual async Task<InstalledAppInstance> GetInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync().ConfigureAwait(false);

            _logger.LogDebug($"Getting installedApp from cache: {installedAppId}...");

            if (InstalledAppCache.TryGetValue(installedAppId, out InstalledAppInstance installedApp))
            {
                _logger.LogDebug($"Got installedApp from cache: {installedAppId}...");
            }
            else
            {
                _logger.LogDebug($"Unable to find installedApp in cache: {installedAppId}...");
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

            _logger.LogDebug($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

            InstalledAppCache.Remove(installedApp.InstalledAppId);
            InstalledAppCache.Add(installedApp.InstalledAppId, installedApp);

            await PersistCacheAsync().ConfigureAwait(false);
        }

        public virtual async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            _logger.LogDebug($"Removing installedApp from cache: {installedAppId}...");

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

            _logger.LogDebug($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            if (installedApp.AccessToken.IsExpired)
            {
                _logger.LogDebug($"installedApp: {installedApp.InstalledAppId} has an expired token, attempting to refresh...");
                installedApp = await _smartThingsAPIHelper.RefreshTokensAsync(installedApp).ConfigureAwait(false);
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
