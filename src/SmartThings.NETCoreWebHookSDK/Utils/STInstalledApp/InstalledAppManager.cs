using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public interface IInstalledAppManager : IHostedService
    {
        ILogger<IInstalledAppManager> logger { get; }
        ISmartThingsAPIHelper smartThingsAPIHelper { get; }
        IDictionary<string, InstalledApp> installedAppCache { get; }
        IDictionary<string, Timer> tokenRefreshTimers { get; }
        Task<InstalledApp> GetInstalledAppAsync(string installedAppId);
        Task StoreInstalledAppAsync(InstalledApp installedApp);
        Task RemoveInstalledAppAsync(string installedAppId);
        Task PersistCacheAsync();
        Task LoadCacheAsync();
    }

    public abstract class InstalledAppManager : IInstalledAppManager
    {
        public ILogger<IInstalledAppManager> logger { get; private set; }
        public ISmartThingsAPIHelper smartThingsAPIHelper { get; private set; }
        public IDictionary<string, InstalledApp> installedAppCache { get; set; }
        public IDictionary<string, Timer> tokenRefreshTimers { get; private set; }

        public InstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = smartThingsAPIHelper ?? throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.logger = logger;
            this.smartThingsAPIHelper = smartThingsAPIHelper;
            this.tokenRefreshTimers = new Dictionary<string, Timer>();
        }

        public virtual async Task<InstalledApp> RefreshTokensAsync(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ?? throw new ArgumentException($"installedApp.InstalledAppId is null", nameof(installedApp));
            _ = installedApp.AccessToken ?? throw new ArgumentException($"installedApp.AccessToken is null", nameof(installedApp));
            _ = installedApp.RefreshToken ?? throw new ArgumentException($"installedApp.RefreshToken is null", nameof(installedApp));

            logger.LogInformation($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            if (installedApp.AccessToken.IsExpired ||
                (installedApp.RefreshToken.IsExpired))
            {
                logger.LogDebug($"installedApp: {installedApp.InstalledAppId} has an expired AuthToken and/or expired RefreshToken, attempting to refresh...");
                installedApp = await smartThingsAPIHelper.RefreshTokensAsync(installedApp);
                await StoreInstalledAppAsync(installedApp);
            }

            return installedApp;
        }

        public virtual async Task<InstalledApp> GetInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Getting installedApp from cache: {installedAppId}...");

            InstalledApp installedApp = null;
            if (installedAppCache.TryGetValue(installedAppId, out installedApp))
            {
                AddTokenTimer(installedApp);
                logger.LogDebug($"Got installedApp from cache: {installedAppId}...");
            }
            else
            {
                logger.LogInformation($"Unable to find installedApp in cache: {installedAppId}...");
            }

            return installedApp;
        }

        private void AddTokenTimer(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
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
            else
            {
                if (!tokenRefreshTimers.ContainsKey(installedApp.InstalledAppId))
                {
                    var tokenRefreshTimer = new Timer((state) =>
                        {
                            RefreshTokensAsync((InstalledApp)state).GetAwaiter().GetResult();
                        },
                        installedApp,
                        TimeSpan.Zero,
                        Token.AccessTokenTTL);

                    tokenRefreshTimers.Add(installedApp.InstalledAppId,
                        tokenRefreshTimer);

                    logger.LogInformation($"Added tokenRefreshTimer for {installedApp.InstalledAppId}...");
                }
            }
        }

        public virtual async Task StoreInstalledAppAsync(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException($"installedApp.InstalledAppId is null",
                nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException($"installedApp.AccessToken is null",
                nameof(installedApp));

            await LoadCacheAsync();

            logger.LogInformation($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

            installedAppCache.Remove(installedApp.InstalledAppId);
            installedAppCache.Add(installedApp.InstalledAppId, installedApp);

            AddTokenTimer(installedApp);

            await PersistCacheAsync();
        }

        public virtual async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Removing installedApp from cache: {installedAppId}...");

            installedAppCache.Remove(installedAppId);
            tokenRefreshTimers.Remove(installedAppId);

            await PersistCacheAsync();
        }

        public abstract Task PersistCacheAsync();

        public abstract Task LoadCacheAsync();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("InstalledAppManager started...");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach(var timer in tokenRefreshTimers.Values)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            logger.LogInformation("InstalledAppManager stopped...");

            return Task.CompletedTask;
        }
    }
}
