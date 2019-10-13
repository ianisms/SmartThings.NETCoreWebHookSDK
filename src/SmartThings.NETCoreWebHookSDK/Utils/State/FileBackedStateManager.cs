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

            _ = fileBackedConfig.BackingStorePath ?? throw new ArgumentException("fileBackedConfig.BackingStorePath is null", nameof(options));
        }

        public override async Task LoadCacheAsync()
        {
            logger.LogInformation("Loading state cache...");

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

                    logger.LogInformation("Loaded state cache from file...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            logger.LogInformation("Saving state cache...");

            Directory.CreateDirectory(Path.GetDirectoryName(fileBackedConfig.BackingStorePath));

            using (var writer = File.CreateText(fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(stateCache);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            logger.LogInformation("Saved state cache...");
        }
    }
}
