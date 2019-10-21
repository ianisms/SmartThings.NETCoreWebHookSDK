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

        public bool LoadedCacheFromStorage { get; set; } = false;

        public AzureStorageBackedStateManager(ILogger<IStateManager<T>> logger,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<T>>> options)
            : base(logger)
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

        public override async Task LoadCacheAsync()
        {
            logger.LogDebug("Loading state cache...");

            LoadedCacheFromStorage = false;

            try
            {
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

                        logger.LogDebug("Loaded state cache from blob...");
                        LoadedCacheFromStorage = true;

                    }
                }
            }
            catch (StorageException stEx)
            {
                logger.LogError(stEx, "Exception trying to load cache from blob...");
                throw;
            }
        }

        public override async Task PersistCacheAsync()
        {
            logger.LogDebug("Saving state cache...");

            try
            {
                var storageClient = storageAccount.CreateCloudBlobClient();
                var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                await container.CreateIfNotExistsAsync();
                var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);
                await cacheBlob.DeleteIfExistsAsync();

                var json = JsonConvert.SerializeObject(stateCache,
                    Common.JsonSerializerSettings);
                await cacheBlob.UploadTextAsync(json);

                logger.LogDebug("Saved state cache...");
            }
            catch (StorageException stEx)
            {
                logger.LogError(stEx, "Exception trying to save cache to blob...");
                throw;
            }
        }
    }
}
