#region Copyright
// <copyright file="AzureStorageBackedInstalledAppManager.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// SmartThings.NETCoreWebHookSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SmartThings.NETCoreWebHookSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
#endregion

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
        private readonly CloudBlobClient storageClient;

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

            if (CloudStorageAccount.TryParse(storageBackedConfig.ConnectionString, out storageAccount))
            {
                storageClient = storageAccount.CreateCloudBlobClient();
            } 
            else
            {
                throw new InvalidOperationException($"Unable to initialize CloudStorageAccount with connection string: {storageBackedConfig.ConnectionString}");
            }
        }

        public AzureStorageBackedInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>> options,
            CloudBlobClient storageClient)
            : base(logger, smartThingsAPIHelper)
        {
            _ = options ??
                throw new ArgumentNullException(nameof(options));
            _ = storageClient ??
                throw new ArgumentNullException(nameof(storageClient));

            this.storageBackedConfig = options.Value;

            _ = this.storageBackedConfig.ContainerName ??
                throw new InvalidOperationException("storageBackedConfig.ContainerName is null");
            _ = this.storageBackedConfig.CacheBlobName ??
                throw new InvalidOperationException("storageBackedConfig.CacheBlobName is null");

            this.storageClient = storageClient;
        }

        public override async Task LoadCacheAsync()
        {
            Logger.LogDebug("Loading installed app cache...");

            if (InstalledAppCache == null)
            {
                var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                await container.CreateIfNotExistsAsync().ConfigureAwait(false);
                var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

                if (!await cacheBlob.ExistsAsync().ConfigureAwait(false))
                {
                    Logger.LogDebug("Backing blob does not exist...");
                    InstalledAppCache = new Dictionary<string, InstalledAppInstance>();
                }
                else
                {
                    Logger.LogDebug("Backing blob exists, loading installed app cache from blob...");

                    var json = await cacheBlob.DownloadTextAsync().ConfigureAwait(false);
                    InstalledAppCache = JsonConvert.DeserializeObject<Dictionary<string, InstalledAppInstance>>(json,
                        STCommon.JsonSerializerSettings);

                    Logger.LogDebug("Loaded installed app cache from blob...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            Logger.LogDebug("Saving installed app cache...");

            var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

            var json = JsonConvert.SerializeObject(InstalledAppCache,
                STCommon.JsonSerializerSettings);
            await cacheBlob.UploadTextAsync(json).ConfigureAwait(false);

            Logger.LogDebug("Saved installed app cache...");
        }
    }
}
