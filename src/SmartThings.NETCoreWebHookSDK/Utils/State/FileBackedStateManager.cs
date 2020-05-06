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
