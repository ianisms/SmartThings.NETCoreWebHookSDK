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

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public class FileBackedInstalledAppManager : InstalledAppManager
    {
        private readonly FileBackedConfig<FileBackedInstalledAppManager> fileBackedConfig;
        private Dictionary<string, InstalledApp> installedAppCache;

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
            logger.LogInformation("Loading installed app cache...");

            if (installedAppCache == null)
            {
                if(!File.Exists(fileBackedConfig.BackingStorePath))
                {
                    logger.LogDebug("Backing file does not exist, initializing intsalled app cache...");

                    installedAppCache = new Dictionary<string, InstalledApp>();
                }
                else
                {
                    logger.LogDebug("Backing file exists, loading installed app cache from file...");

                    using (var reader = File.OpenText(fileBackedConfig.BackingStorePath))
                    {
                        var encodedContent = await reader.ReadToEndAsync();
                        //var json = dataProtector.Unprotect(encodedContent);
                        var json = encodedContent;
                        installedAppCache = JsonConvert.DeserializeObject<Dictionary<string, InstalledApp>>(json,
                            Common.JsonSerializerSettings);
                    }

                    logger.LogInformation("Loaded installed app cache from file...");

                }
            }
        }

        public override async Task PersistCacheAsync()
        {
            logger.LogInformation("Saving installed app cache...");

            Directory.CreateDirectory(Path.GetDirectoryName(fileBackedConfig.BackingStorePath));

            using (var writer = File.CreateText(fileBackedConfig.BackingStorePath))
            {
                var json = JsonConvert.SerializeObject(installedAppCache,
                    Common.JsonSerializerSettings);
                var encodedContent = json;
                //var encodedContent = dataProtector.Protect(json);
                await writer.WriteAsync(encodedContent).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            logger.LogInformation("Saved installed app cache...");
        }
    }
}
