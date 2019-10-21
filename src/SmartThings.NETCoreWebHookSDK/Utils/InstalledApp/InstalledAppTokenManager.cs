using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public interface IInstalledAppTokenManager : IHostedService
    {
    }

    public class InstalledAppTokenManager : IInstalledAppTokenManager
    {
        private readonly ILogger<IInstalledAppTokenManager> logger;
        private readonly IInstalledAppManager installedAppManager;
        private Timer refreshTimer;

        public InstalledAppTokenManager(ILogger<IInstalledAppTokenManager> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ?? throw new ArgumentNullException(nameof(installedAppManager));

            this.logger = logger;
            this.installedAppManager = installedAppManager;
        }

        private async Task RefreshAllTokensAsync()
        {
            logger.LogDebug("Refreshing all tokens...");

            try
            {
                await installedAppManager.RefreshAllInstalledAppTokensAsync();
                logger.LogDebug("All tokens refreshed...");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception trying to refresh all tokens!");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            refreshTimer = new Timer(async (object state) =>
            {
                await RefreshAllTokensAsync();
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(4));

            logger.LogDebug("InstalledAppTokenManager started...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            refreshTimer.Dispose();
            logger.LogDebug("InstalledAppTokenManager stopped...");
        }
    }
}
