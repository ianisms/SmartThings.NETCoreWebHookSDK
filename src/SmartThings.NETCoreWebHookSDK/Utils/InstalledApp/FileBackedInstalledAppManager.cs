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
    public class FileBackedInstalledAppManager : InstalledAppManager
    {
        private readonly FileBackedConfig<FileBackedInstalledAppManager> fileBackedConfig;

        public FileBackedInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IOptions<FileBackedConfig<FileBackedInstalledAppManager>> options)
            : base (logger, smartThingsAPIHelper)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            this.fileBackedConfig = options.Value;

            _ = fileBackedConfig.BackingStorePath ?? throw new ArgumentException("fileBackedConfig.BackingStorePath is null", nameof(options));
        }

        public override async Task LoadCacheAsync()
        {
            Logger.LogInformation("Loading installed app cache...");

            if (InstalledAppCache == null)
            {
                if(!File.Exists(fileBackedConfig.BackingStorePath))
                {
                    Logger.LogDebug("Backing file does not exist, initializing intsalled app cache...");

                    InstalledAppCache = new Dictionary<string, Models.SmartThings.InstalledApp>();
                }
                else
                {
                    Logger.LogDebug("Backing file exists, loading installed app cache from file...");

                    using (var reader = File.OpenText(fileBackedConfig.BackingStorePath))
                    {
                        var encodedContent = await reader.ReadToEndAsync();
                        //var json = dataProtector.Unprotect(encodedContent);
                        var json = encodedContent;
                        InstalledAppCache = JsonConvert.DeserializeObject<Dictionary<string, Models.SmartThings.InstalledApp>>(json,
                            Common.JsonSerializerSettings);
                    }

                    Logger.LogInformation("Loaded installed app cache from file...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            Logger.LogInformation("Saving installed app cache...");

            Directory.CreateDirectory(Path.GetDirectoryName(fileBackedConfig.BackingStorePath));

            using (var writer = File.CreateText(fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(InstalledAppCache,
                    Common.JsonSerializerSettings);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            Logger.LogInformation("Saved installed app cache...");
        }
    }
}
