using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public class InMemoryInstalledAppManager : InstalledAppManager
    {
        private Dictionary<string, InstalledApp> installedAppCache;

        public InMemoryInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
            : base (logger, smartThingsAPIHelper)
        {
            installedAppCache = new Dictionary<string, InstalledApp>();
        }

        public override async Task RefreshTokensAsync(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException($"installedApp.InstalledAppId is null", nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException($"installedApp.AccessToken is null", nameof(installedApp));

            logger.LogInformation($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            if (installedApp.AccessToken.IsExpired ||
                (installedApp.RefreshToken == null || installedApp.RefreshToken.IsExpired))
            {
                logger.LogDebug($"installedApp: {installedApp.InstalledAppId} has an expired AuthToken and/or a null or expired RefreshToken, attempting to refresh...");
                installedApp = await smartThingsAPIHelper.RefreshTokenAsync(installedApp);
                installedAppCache.Remove(installedApp.InstalledAppId);
                installedAppCache.Add(installedApp.InstalledAppId, installedApp);
            }
        }

        public override async Task<InstalledApp> GetInstalledAppAsync(string installedAppId,
            bool shouldRefreshTokens = true)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Getting installedApp from cache: {installedAppId}...");

            InstalledApp installedApp = null;
            if (installedAppCache.TryGetValue(installedAppId, out installedApp))
            {
                logger.LogDebug($"Got installedApp from cache: {installedAppId}...");

                if (shouldRefreshTokens)
                {
                    //await RefreshTokensAsync(installedApp);
                }
            }
            else
            {
                logger.LogInformation($"Unable to find installedApp in cache: {installedAppId}...");
            }

            return installedApp;
        }

        public override async Task StoreInstalledAppAsync(InstalledApp installedApp,
            bool shouldRefreshTokens = true)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ??
                throw new ArgumentException($"installedApp.InstalledAppId is null", nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException($"installedApp.AccessToken is null", nameof(installedApp));

            if (shouldRefreshTokens)
            {
                await RefreshTokensAsync(installedApp);
            }

            logger.LogInformation($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

            installedAppCache.Remove(installedApp.InstalledAppId);
            installedAppCache.Add(installedApp.InstalledAppId, installedApp);
        }

        public override async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Removing installedApp from cache: {installedAppId}...");

            installedAppCache.Remove(installedAppId);
        }
    }
}
