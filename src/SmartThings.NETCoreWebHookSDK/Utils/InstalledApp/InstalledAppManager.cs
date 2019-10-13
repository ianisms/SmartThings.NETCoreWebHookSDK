using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public interface IInstalledAppManager
    {
        ILogger<IInstalledAppManager> Logger { get; }
        ISmartThingsAPIHelper SmartThingsAPIHelper { get; }
        IDictionary<string, Models.SmartThings.InstalledApp> InstalledAppCache { get; }
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
        public IDictionary<string, Timer> tokenRefreshTimers { get; private set; }

        public InstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = smartThingsAPIHelper ?? throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.Logger = logger;
            this.SmartThingsAPIHelper = smartThingsAPIHelper;
            this.tokenRefreshTimers = new Dictionary<string, Timer>();
        }

        public virtual async Task<Models.SmartThings.InstalledApp> GetInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            Logger.LogInformation($"Getting installedApp from cache: {installedAppId}...");

            Models.SmartThings.InstalledApp installedApp = null;
            if (InstalledAppCache.TryGetValue(installedAppId, out installedApp))
            {
                Logger.LogDebug($"Got installedApp from cache: {installedAppId}...");
            }
            else
            {
                Logger.LogInformation($"Unable to find installedApp in cache: {installedAppId}...");
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

            Logger.LogInformation($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

            InstalledAppCache.Remove(installedApp.InstalledAppId);
            InstalledAppCache.Add(installedApp.InstalledAppId, installedApp);

            await PersistCacheAsync();
        }

        public virtual async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            Logger.LogInformation($"Removing installedApp from cache: {installedAppId}...");

            InstalledAppCache.Remove(installedAppId);
            tokenRefreshTimers.Remove(installedAppId);

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

            Logger.LogInformation($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

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
    }
}
