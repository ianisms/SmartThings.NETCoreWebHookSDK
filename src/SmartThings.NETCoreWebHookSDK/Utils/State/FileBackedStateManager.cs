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
using FluentValidation;
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
        private readonly ILogger<IStateManager<T>> _logger;
        private readonly FileBackedConfig<FileBackedStateManager<T>> _fileBackedConfig;
        private readonly FileBackedConfigValidator<FileBackedStateManager<T>> _fileBackedConfigValidator;
        private readonly IFileSystem _fileSystem;

        public FileBackedStateManager(ILogger<IStateManager<T>> logger,
            IOptions<FileBackedConfig<FileBackedStateManager<T>>> options,
            FileBackedConfigValidator<FileBackedStateManager<T>> fileBackedConfigValidator,
            IFileSystem fileSystem)
            : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileBackedConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _fileBackedConfigValidator = fileBackedConfigValidator ?? throw new ArgumentNullException(nameof(fileBackedConfigValidator));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            _fileBackedConfigValidator.ValidateAndThrow(_fileBackedConfig);
        }

        public override async Task LoadCacheAsync()
        {
            _logger.LogDebug("Loading state cache...");

            if (StateCache == null)
            {
                if (!_fileSystem.File.Exists(_fileBackedConfig.BackingStorePath))
                {
                    _logger.LogDebug("Backing file does not exist, initializing cache...");

                    StateCache = new Dictionary<string, T>();
                }
                else
                {
                    _logger.LogDebug("Backing file exists, loading cache from file...");

                    using (var reader = _fileSystem.File.OpenText(_fileBackedConfig.BackingStorePath))
                    {
                        var encodedContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                        //var json = dataProtector.Unprotect(encodedContent);
                        var json = encodedContent;
                        StateCache = JsonConvert.DeserializeObject<Dictionary<string, T>>(json,
                            STCommon.JsonSerializerSettings);
                    }

                    _logger.LogDebug("Loaded state cache from file...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            _logger.LogDebug("Saving state cache...");

            Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(_fileBackedConfig.BackingStorePath));

            using (var writer = _fileSystem.File.CreateText(_fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(StateCache);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            _logger.LogDebug("Saved state cache...");
        }
    }
}
