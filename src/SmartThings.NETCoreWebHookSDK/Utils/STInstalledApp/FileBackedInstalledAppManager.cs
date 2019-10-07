using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public class FileBackedInstalledAppManager : InstalledAppManager
    {
        private readonly FileBackedConfig<FileBackedInstalledAppManager> fileBackedConfig;
        private Dictionary<string, InstalledApp> installedAppCache;

        public FileBackedInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IOptions<FileBackedConfig<FileBackedInstalledAppManager>> options)
            : base (logger, smartThingsAPIHelper)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            this.fileBackedConfig = options.Value;

            _ = fileBackedConfig.BackingStorePath ?? throw new ArgumentException("fileBackedConfig.BackingStorePath is null", nameof(options));
        }

        private async Task LoadCacheAsync()
        {
            logger.LogInformation("Loading installed app cache...");

            if (installedAppCache == null)
            {
                if(!File.Exists(fileBackedConfig.BackingStorePath))
                {
                    logger.LogDebug("Backing file does not exist, initializing intsalled app cache...");

                    installedAppCache = new Dictionary<string, InstalledApp>();
                }
                else
                {
                    logger.LogDebug("Backing file exists, loading installed app cache from file...");

                    using (var reader = File.OpenText(fileBackedConfig.BackingStorePath))
                    {
                        var encodedContent = await reader.ReadToEndAsync();
                        //var json = dataProtector.Unprotect(encodedContent);
                        var json = encodedContent;
                        installedAppCache = JsonConvert.DeserializeObject<Dictionary<string, InstalledApp>>(json,
                            Common.JsonSerializerSettings);
                    }

                    logger.LogInformation("Loaded installed app cache from file...");

                }
            }
        }

        private async Task PersistCacheAsync()
        {
            logger.LogInformation("Saving installed app cache...");

            Directory.CreateDirectory(Path.GetDirectoryName(fileBackedConfig.BackingStorePath));

            using (var writer = File.CreateText(fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(installedAppCache,
                    Common.JsonSerializerSettings);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            logger.LogInformation("Saved installed app cache...");
        }

        public override async Task RefreshTokensAsync(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.InstalledAppId ?? throw new ArgumentException($"installedApp.InstalledAppId is null", nameof(installedApp));
            _ = installedApp.AccessToken ?? throw new ArgumentException($"installedApp.AccessToken is null", nameof(installedApp));

            logger.LogInformation($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            if (installedApp.AccessToken.IsExpired ||
                (installedApp.RefreshToken == null || installedApp.RefreshToken.IsExpired))
            {
                logger.LogDebug($"installedApp: {installedApp.InstalledAppId} has an expired AuthToken and/or a null or expired RefreshToken, attempting to refresh...");
                installedApp = await smartThingsAPIHelper.RefreshTokenAsync(installedApp);

                installedAppCache.Remove(installedApp.InstalledAppId);
                installedAppCache.Add(installedApp.InstalledAppId, installedApp);
                await PersistCacheAsync();
            }
        }

        public override async Task<InstalledApp> GetInstalledAppAsync(string installedAppId,
            bool shouldRefreshTokens = true)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Getting installedApp from cache: {installedAppId}...");

            InstalledApp installedApp = null;
            if(installedAppCache.TryGetValue(installedAppId, out installedApp))
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
            _ = installedApp.InstalledAppId ?? throw new ArgumentException($"installedApp.InstalledAppId is null", nameof(installedApp));
            _ = installedApp.AccessToken ?? throw new ArgumentException($"installedApp.AccessToken is null", nameof(installedApp));

            await LoadCacheAsync();

            if (shouldRefreshTokens)
            {
                await RefreshTokensAsync(installedApp);
            }

            logger.LogDebug($"adding installedApp to cache: {installedApp.InstalledAppId}...");

            installedAppCache.Remove(installedApp.InstalledAppId);
            installedAppCache.Add(installedApp.InstalledAppId, installedApp);

            await PersistCacheAsync();
        }

        public override async Task RemoveInstalledAppAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Removing installedApp from cache: {installedAppId}...");

            installedAppCache.Remove(installedAppId);

            await PersistCacheAsync();
        }
    }
}
