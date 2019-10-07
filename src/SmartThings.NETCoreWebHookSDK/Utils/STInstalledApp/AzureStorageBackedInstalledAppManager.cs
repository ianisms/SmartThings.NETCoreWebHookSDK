using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public class AzureStorageBackedInstalledAppManager : InstalledAppManager
    {
        private readonly AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager> storageBackedConfig;
        private readonly CloudStorageAccount storageAccount;
        private Dictionary<string, InstalledApp> installedAppCache { get; set; }

        public AzureStorageBackedInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>> options)
            : base(logger, smartThingsAPIHelper)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            this.storageBackedConfig = options.Value;

            _ = this.storageBackedConfig.ConnectionString ?? 
                throw new InvalidOperationException("storageBackedConfig.ConnectionString is null");
            _ = this.storageBackedConfig.ContainerName ?? 
                throw new InvalidOperationException("storageBackedConfig.ContainerName is null");
            _ = this.storageBackedConfig.CacheBlobName ??
                throw new InvalidOperationException("storageBackedConfig.CacheBlobName is null");

            if (!CloudStorageAccount.TryParse(storageBackedConfig.ConnectionString, out storageAccount))
            {
                throw new InvalidOperationException($"Unable to initialize CloudStorageAccount with connection string: {storageBackedConfig.ConnectionString}");
            }
        }

        private async Task LoadCacheAsync()
        {
            logger.LogInformation("Loading installed app cache...");

            if (installedAppCache == null)
            {
                var storageClient = storageAccount.CreateCloudBlobClient();
                var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                await container.CreateIfNotExistsAsync();
                var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

                if (!await cacheBlob.ExistsAsync())
                {
                    logger.LogDebug("Backing blob does not exist, initializing cache...");

                    installedAppCache = new Dictionary<string, InstalledApp>();
                }
                else
                {
                    logger.LogDebug("Backing blob exists, loading installed app cache from blob...");

                    var json = await cacheBlob.DownloadTextAsync();
                    installedAppCache = JsonConvert.DeserializeObject<Dictionary<string, InstalledApp>>(json,
                        Common.JsonSerializerSettings);

                    logger.LogInformation("Loaded installed app cache from blob...");

                }
            }
        }

        private async Task PersistCacheAsync()
        {
            logger.LogInformation("Saving installed app cache...");

            var storageClient = storageAccount.CreateCloudBlobClient();
            var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
            await container.CreateIfNotExistsAsync();
            var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

            var json = JsonConvert.SerializeObject(installedAppCache,
                Common.JsonSerializerSettings);
            await cacheBlob.UploadTextAsync(json);

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

            await LoadCacheAsync();

            if (shouldRefreshTokens)
            {
                await RefreshTokensAsync(installedApp);
            }

            logger.LogInformation($"Adding installedApp to cache: {installedApp.InstalledAppId}...");

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
