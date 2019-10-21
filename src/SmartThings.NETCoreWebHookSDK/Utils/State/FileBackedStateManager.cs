#region Copyright
// <copyright file="FileBackedStateManager.cs" company="Ian N. Bennett">
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public class FileBackedStateManager<T> : StateManager<T>
    {
        private readonly FileBackedConfig<FileBackedStateManager<T>> fileBackedConfig;
        private Dictionary<string, T> stateCache { get; set; }

        public FileBackedStateManager(ILogger<IStateManager<T>> logger,
            IOptions<FileBackedConfig<FileBackedStateManager<T>>> options)
            : base (logger)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            this.fileBackedConfig = options.Value;

            _ = fileBackedConfig.BackingStorePath ??
                throw new ArgumentException("fileBackedConfig.BackingStorePath is null", nameof(options));
        }

        public override async Task LoadCacheAsync()
        {
            logger.LogDebug("Loading state cache...");

            if (stateCache == null)
            {
                if(!File.Exists(fileBackedConfig.BackingStorePath))
                {
                    logger.LogDebug("Backing file does not exist, initializing cache...");

                    stateCache = new Dictionary<string, T>();
                }
                else
                {
                    logger.LogDebug("Backing file exists, loading cache from file...");

                    using (var reader = File.OpenText(fileBackedConfig.BackingStorePath))
                    {
                        var encodedContent = await reader.ReadToEndAsync();
                        //var json = dataProtector.Unprotect(encodedContent);
                        var json = encodedContent;
                        stateCache = JsonConvert.DeserializeObject<Dictionary<string, T>>(json,
                            Common.JsonSerializerSettings);
                    }

                    logger.LogDebug("Loaded state cache from file...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            logger.LogDebug("Saving state cache...");

            Directory.CreateDirectory(Path.GetDirectoryName(fileBackedConfig.BackingStorePath));

            using (var writer = File.CreateText(fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(stateCache);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            logger.LogDebug("Saved state cache...");
        }
    }
}
