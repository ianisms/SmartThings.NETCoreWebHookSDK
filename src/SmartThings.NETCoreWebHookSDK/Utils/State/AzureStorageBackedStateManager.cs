#region Copyright
// <copyright file="AzureStorageBackedStateManager.cs" company="Ian N. Bennett">
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
            Logger.LogDebug("Loading state cache...");

            LoadedCacheFromStorage = false;

            try
            {
                if (StateCache == null)
                {
                    var storageClient = storageAccount.CreateCloudBlobClient();
                    var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                    await container.CreateIfNotExistsAsync().ConfigureAwait(false);
                    var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);

                    if (!await cacheBlob.ExistsAsync().ConfigureAwait(false))
                    {
                        Logger.LogDebug("Backing blob does not exist, initializing cache...");

                        StateCache = new Dictionary<string, T>();
                    }
                    else
                    {
                        Logger.LogDebug("Backing blob exists, loading cache from blob...");

                        var json = await cacheBlob.DownloadTextAsync().ConfigureAwait(false);
                        StateCache = JsonConvert.DeserializeObject<Dictionary<string, T>>(json,
                            STCommon.JsonSerializerSettings);

                        Logger.LogDebug("Loaded state cache from blob...");
                        LoadedCacheFromStorage = true;

                    }
                }
            }
            catch (StorageException stEx)
            {
                Logger.LogError(stEx, "Exception trying to load cache from blob...");
                throw;
            }
        }

        public override async Task PersistCacheAsync()
        {
            Logger.LogDebug("Saving state cache...");

            try
            {
                var storageClient = storageAccount.CreateCloudBlobClient();
                var container = storageClient.GetContainerReference(storageBackedConfig.ContainerName);
                await container.CreateIfNotExistsAsync().ConfigureAwait(false);
                var cacheBlob = container.GetBlockBlobReference(storageBackedConfig.CacheBlobName);
                await cacheBlob.DeleteIfExistsAsync().ConfigureAwait(false);

                var json = JsonConvert.SerializeObject(StateCache,
                    STCommon.JsonSerializerSettings);
                await cacheBlob.UploadTextAsync(json).ConfigureAwait(false);

                Logger.LogDebug("Saved state cache...");
            }
            catch (StorageException stEx)
            {
                Logger.LogError(stEx, "Exception trying to save cache to blob...");
                throw;
            }
        }
    }
}
