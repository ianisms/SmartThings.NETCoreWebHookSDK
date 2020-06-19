#region Copyright
// <copyright file="AzureStorageBackedStateManager.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
        private readonly CloudBlobClient storageClient;

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

            if (CloudStorageAccount.TryParse(storageBackedConfig.ConnectionString, out storageAccount))
            {
                storageClient = storageAccount.CreateCloudBlobClient();
            }
            else
            {
                throw new InvalidOperationException($"Unable to initialize CloudStorageAccount with connection string: {storageBackedConfig.ConnectionString}");
            }
        }

        public AzureStorageBackedStateManager(ILogger<IStateManager<T>> logger,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<T>>> options,
            CloudBlobClient storageClient)
            : base(logger)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
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
            Logger.LogDebug("Loading state cache...");

            LoadedCacheFromStorage = false;

            try
            {
                if (StateCache == null)
                {
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
