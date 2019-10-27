using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public interface IInstalledAppTokenManager
    {
        ILogger<IInstalledAppTokenManager> Logger { get; }
        IInstalledAppManager InstalledAppManager { get; }
        Task RefreshAllTokensAsync();
    }

    public class InstalledAppTokenManager : IInstalledAppTokenManager
    {
        public ILogger<IInstalledAppTokenManager> Logger { get; private set; }
        public IInstalledAppManager InstalledAppManager { get; private set; }

        public InstalledAppTokenManager(ILogger<IInstalledAppTokenManager> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ?? throw new ArgumentNullException(nameof(installedAppManager));

            this.Logger = logger;
            this.InstalledAppManager = installedAppManager;
        }

        public async Task RefreshAllTokensAsync()
        {
            Logger.LogDebug("Refreshing all tokens...");

            try
            {
                await InstalledAppManager.RefreshAllInstalledAppTokensAsync().ConfigureAwait(false);
                Logger.LogDebug("All tokens refreshed...");
            }
            catch (AggregateException ex)
            {
                Logger.LogError(ex, "Exception trying to refresh all tokens!");
            }
        }
    }
}
