using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public interface IInstalledAppManager
    {
        ILogger<IInstalledAppManager> logger { get; }
        ISmartThingsAPIHelper smartThingsAPIHelper { get; }
        Task<InstalledApp> GetInstalledAppAsync(string installedAppId,
            bool shouldRefreshTokens = true);
        Task StoreInstalledAppAsync(InstalledApp installedApp,
            bool shouldRefreshTokens = true);
        Task RemoveInstalledAppAsync(string installedAppId);
    }

    public abstract class InstalledAppManager : IInstalledAppManager
    {
        public ILogger<IInstalledAppManager> logger { get; private set; }
        public ISmartThingsAPIHelper smartThingsAPIHelper { get; private set; }

        public InstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = smartThingsAPIHelper ?? throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.logger = logger;
            this.smartThingsAPIHelper = smartThingsAPIHelper;
        }

        public abstract Task RefreshTokensAsync(InstalledApp installedApp);

        public abstract Task<InstalledApp> GetInstalledAppAsync(string installedAppId,
            bool shouldRefreshTokens = true);

        public abstract Task StoreInstalledAppAsync(InstalledApp installedApp,
            bool shouldRefreshTokens = true);

        public abstract Task RemoveInstalledAppAsync(string installedAppId);
    }
}
