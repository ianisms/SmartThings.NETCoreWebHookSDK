#region Copyright
// <copyright file="FileBackedStateManager.cs" company="Ian N. Bennett">
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public class FileBackedStateManager<T> : StateManager<T>
    {
        private readonly FileBackedConfig<FileBackedStateManager<T>> fileBackedConfig;
        private readonly IFileSystem fileSystem;

        public FileBackedStateManager(ILogger<IStateManager<T>> logger,
            IOptions<FileBackedConfig<FileBackedStateManager<T>>> options,
            IFileSystem fileSystem)
            : base(logger)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            this.fileBackedConfig = options.Value;
            this.fileSystem = fileSystem;

            _ = fileBackedConfig.BackingStorePath ??
                throw new ArgumentException("fileBackedConfig.BackingStorePath is null",
                nameof(options));
        }

        public override async Task LoadCacheAsync()
        {
            Logger.LogDebug("Loading state cache...");

            if (StateCache == null)
            {
                if (!fileSystem.File.Exists(fileBackedConfig.BackingStorePath))
                {
                    Logger.LogDebug("Backing file does not exist, initializing cache...");

                    StateCache = new Dictionary<string, T>();
                }
                else
                {
                    Logger.LogDebug("Backing file exists, loading cache from file...");

                    using (var reader = fileSystem.File.OpenText(fileBackedConfig.BackingStorePath))
                    {
                        var encodedContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                        //var json = dataProtector.Unprotect(encodedContent);
                        var json = encodedContent;
                        StateCache = JsonConvert.DeserializeObject<Dictionary<string, T>>(json,
                            STCommon.JsonSerializerSettings);
                    }

                    Logger.LogDebug("Loaded state cache from file...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            Logger.LogDebug("Saving state cache...");

            Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(fileBackedConfig.BackingStorePath));

            using (var writer = fileSystem.File.CreateText(fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(StateCache);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            Logger.LogDebug("Saved state cache...");
        }
    }
}
