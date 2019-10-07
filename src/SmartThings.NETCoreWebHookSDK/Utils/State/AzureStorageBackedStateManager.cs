using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public class AzureStorageBackedStateManager<T> : StateManager<T>
    {
        private readonly AzureStorageBackedConfig<AzureStorageBackedStateManager<T>> storageBackedConfig;
        private readonly CloudStorageAccount storageAccount;
        private Dictionary<string, T> stateCache { get; set; }

        public AzureStorageBackedStateManager(ILogger<IStateManager<T>> logger,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<T>>> options)
            : base (logger)
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
            logger.LogInformation("Loading state cache...");

            if (stateCache == null)
            {
                var storageClient = storageAccount.CreateCloudBlobClient();
                var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                await container.CreateIfNotExistsAsync();
                var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

                if (!await cacheBlob.ExistsAsync())
                {
                    logger.LogDebug("Backing blob does not exist, initializing cache...");

                    stateCache = new Dictionary<string, T>();
                }
                else
                {
                    logger.LogDebug("Backing blob exists, loading cache from blob...");

                    var json = await cacheBlob.DownloadTextAsync();
                    stateCache = JsonConvert.DeserializeObject<Dictionary<string, T>>(json,
                        Common.JsonSerializerSettings);

                    logger.LogInformation("Loaded state cache from blob...");

                }
            }
        }

        private async Task PersistCacheAsync()
        {
            logger.LogInformation("Saving state cache...");

            var storageClient = storageAccount.CreateCloudBlobClient();
            var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
            await container.CreateIfNotExistsAsync();
            var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

            var json = JsonConvert.SerializeObject(stateCache,
                Common.JsonSerializerSettings);
            await cacheBlob.UploadTextAsync(json);

            logger.LogInformation("Saved state cache...");
        }

        public override async Task<T> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Getting state from cache: {installedAppId}...");

            T state = default(T);
            if (stateCache.TryGetValue(installedAppId, out state))
            {
                logger.LogDebug($"Got state from cache: {installedAppId}...");
            }
            else
            {
                logger.LogInformation($"Unable to find state in cache: {installedAppId}...");
            }

            return state;
        }

        public override async Task StoreStateAsync(string installedAppId, T state)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Adding state to cache: {installedAppId}...");

            stateCache.Remove(installedAppId);
            stateCache.Add(installedAppId, state);

            await PersistCacheAsync();
        }

        public override async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Removing state from cache: {installedAppId}...");

            stateCache.Remove(installedAppId);

            await PersistCacheAsync();
        }
    }
}
