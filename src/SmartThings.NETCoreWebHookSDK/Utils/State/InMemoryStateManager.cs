using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public class InMemoryStateManager<T> : StateManager<T>
    {
        private Dictionary<string, T> stateCache { get; set; }

        public InMemoryStateManager(ILogger<IStateManager<T>> logger)
            : base(logger)
        {
            stateCache = new Dictionary<string, T>();
        }

        public override async Task<T> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Getting state for installedApp: {installedAppId}...");

            T state = default(T);
            if (!stateCache.TryGetValue(installedAppId, out state))
            {
                logger.LogInformation($"State not found for installedApp: {installedAppId}...");
            }

            return state;
        }

        public override async Task StoreStateAsync(string installedAppId, T state)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Storing state for installedApp: {installedAppId}...");

            stateCache.Remove(installedAppId);
            stateCache.Add(installedAppId, state);
        }

        public override async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            logger.LogInformation($"Removing state from cache: {installedAppId}...");

            stateCache.Remove(installedAppId);
        }
    }
}
