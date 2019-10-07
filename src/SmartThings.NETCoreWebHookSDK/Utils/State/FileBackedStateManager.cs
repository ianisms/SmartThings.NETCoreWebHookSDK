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

        private async Task LoadCacheAsync()
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

        private async Task PersistCacheAsync()
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

        public override async Task<T> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation("Getting state from cache: {installedAppId}...");

            T state = default(T);
            if (stateCache.TryGetValue(installedAppId, out state))
            {
                logger.LogDebug($"Got state from cache: {installedAppId}...");
            }
            else
            {
                logger.LogInformation($"Unable to find state in cache: {installedAppId}...");
            }

            return state;
        }

        public override async Task StoreStateAsync(string installedAppId, T state)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Adding state to cache: {installedAppId}...");

            stateCache.Remove(installedAppId);
            stateCache.Add(installedAppId, state);

            await PersistCacheAsync();
        }

        public override async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            await LoadCacheAsync();

            logger.LogInformation($"Removing state from cache: {installedAppId}...");

            stateCache.Remove(installedAppId);

            await PersistCacheAsync();
        }
    }
}
