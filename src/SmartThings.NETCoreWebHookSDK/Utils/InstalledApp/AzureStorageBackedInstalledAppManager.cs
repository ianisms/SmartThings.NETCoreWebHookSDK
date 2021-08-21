#region Copyright
// <copyright file="AzureStorageBackedInstalledAppManager.cs" company="Ian N. Bennett">
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
using Azure.Storage.Blobs;
using FluentValidation;
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

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public class AzureStorageBackedInstalledAppManager : InstalledAppManager
    {
        private readonly ILogger<IInstalledAppManager> _logger;
        private readonly AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager> _storageBackedConfig;
        private readonly AzureStorageBackedConfigValidator<AzureStorageBackedInstalledAppManager> _storageBackedConfigValidator;
        private readonly AzureStorageBackedConfigWithClientValidator<AzureStorageBackedInstalledAppManager> _storageBackedConfigWithClientValidator;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorageBackedInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>> options,
            AzureStorageBackedConfigValidator<AzureStorageBackedInstalledAppManager> storageBackedConfigValidator)
            : base(logger, smartThingsAPIHelper)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _storageBackedConfig = options?.Value ??
                throw new ArgumentNullException(nameof(options));
            _storageBackedConfigValidator = storageBackedConfigValidator ??
                throw new ArgumentNullException(nameof(storageBackedConfigValidator)); 

            _storageBackedConfigValidator.ValidateAndThrow(_storageBackedConfig);

            _blobServiceClient = new BlobServiceClient(_storageBackedConfig.ConnectionString);
        }

        public AzureStorageBackedInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>> options,
            AzureStorageBackedConfigWithClientValidator<AzureStorageBackedInstalledAppManager> storageBackedConfigWithClientValidator,
            BlobServiceClient blobServiceClient)
            : base(logger, smartThingsAPIHelper)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _storageBackedConfig = options?.Value ??
                throw new ArgumentNullException(nameof(options));
            _storageBackedConfigWithClientValidator = storageBackedConfigWithClientValidator ??
                throw new ArgumentNullException(nameof(storageBackedConfigWithClientValidator));
            _blobServiceClient = blobServiceClient ??
                throw new ArgumentNullException(nameof(blobServiceClient));

            _storageBackedConfigWithClientValidator.ValidateAndThrow(_storageBackedConfig);
        }

        public override async Task LoadCacheAsync()
        {
            _logger.LogDebug("Loading installed app cache...");

            if (InstalledAppCache == null)
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_storageBackedConfig.ContainerName);
                await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

                var blobClient = containerClient.GetBlobClient(_storageBackedConfig.CacheBlobName);

                if (!await blobClient.ExistsAsync().ConfigureAwait(false))
                {
                    _logger.LogDebug("Backing blob does not exist...");
                    InstalledAppCache = new Dictionary<string, InstalledAppInstance>();
                }
                else
                {
                    _logger.LogDebug("Backing blob exists, loading installed app cache from blob...");

                    var blobInfo = await blobClient.DownloadAsync();

                    if (blobInfo != null &&
                        blobInfo.Value != null &&
                        blobInfo.Value.ContentLength > 0)
                    {
                        using var reader = new StreamReader(blobInfo.Value.Content);
                        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

                        InstalledAppCache = JsonConvert.DeserializeObject<Dictionary<string, InstalledAppInstance>>(json,
                            STCommon.JsonSerializerSettings);

                        _logger.LogDebug("Loaded installed app cache from blob...");
                    }
                    else
                    {
                        _logger.LogDebug("Backing blob was empty...");
                        InstalledAppCache = new Dictionary<string, InstalledAppInstance>();
                    }
                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            _logger.LogDebug("Saving installed app cache...");

            var containerClient = _blobServiceClient.GetBlobContainerClient(_storageBackedConfig.ContainerName);
            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            var blobClient = containerClient.GetBlobClient(_storageBackedConfig.CacheBlobName);

            var json = JsonConvert.SerializeObject(InstalledAppCache,
                STCommon.JsonSerializerSettings);

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, true);
            await writer.WriteAsync(json);
            writer.Flush();
            stream.Position = 0;

            await blobClient.UploadAsync(stream).ConfigureAwait(false);

            _logger.LogDebug("Saved installed app cache...");
        }
    }
}
