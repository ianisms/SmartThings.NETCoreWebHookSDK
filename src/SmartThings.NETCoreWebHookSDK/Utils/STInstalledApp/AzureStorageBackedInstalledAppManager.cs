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

        public override async Task LoadCacheAsync()
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
                    logger.LogDebug("Backing blob does not exist...");
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

        public override async Task PersistCacheAsync()
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
    }
}
