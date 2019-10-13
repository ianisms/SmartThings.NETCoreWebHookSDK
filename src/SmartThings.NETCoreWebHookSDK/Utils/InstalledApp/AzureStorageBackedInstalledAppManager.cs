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

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
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
            Logger.LogInformation("Loading installed app cache...");

            if (InstalledAppCache == null)
            {
                var storageClient = storageAccount.CreateCloudBlobClient();
                var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                await container.CreateIfNotExistsAsync();
                var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

                if (!await cacheBlob.ExistsAsync())
                {
                    Logger.LogDebug("Backing blob does not exist...");
                    InstalledAppCache = new Dictionary<string, Models.SmartThings.InstalledApp>();
                }
                else
                {
                    Logger.LogDebug("Backing blob exists, loading installed app cache from blob...");

                    var json = await cacheBlob.DownloadTextAsync();
                    InstalledAppCache = JsonConvert.DeserializeObject<Dictionary<string, Models.SmartThings.InstalledApp>>(json,
                        Common.JsonSerializerSettings);

                    Logger.LogInformation("Loaded installed app cache from blob...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            Logger.LogInformation("Saving installed app cache...");

            var storageClient = storageAccount.CreateCloudBlobClient();
            var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
            await container.CreateIfNotExistsAsync();
            var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

            var json = JsonConvert.SerializeObject(InstalledAppCache,
                Common.JsonSerializerSettings);
            await cacheBlob.UploadTextAsync(json);

            Logger.LogInformation("Saved installed app cache...");
        }
    }
}
