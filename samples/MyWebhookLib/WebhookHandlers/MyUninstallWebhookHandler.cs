using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace MyWebhookLib.WebhookHandlers
{
    public class MyUninstallWebhookHandler : UninstallWebhookHandler
    {
        private readonly IStateManager<MyState> stateManager;

        public MyUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger,
            IInstalledAppManager installedAppManager,
            IStateManager<MyState> stateManager)
            : base(logger, installedAppManager)
        {
            _ = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            _ = request.uninstallData.installedApp.config.isAppEnabled ??
                throw new InvalidOperationException("request.uninstallData.installedApp.config.isAppEnabled is null");
            _ = request.uninstallData.installedApp.config.presenceSensors ??
                throw new InvalidOperationException("request.uninstallData.installedApp.config.presenceSensors is null");
        }

        public override async Task HandleUninstallDataAsync(dynamic uninstallData)
        {
            var installedAppId = uninstallData.installedApp.InstalledAppId;
            await stateManager.RemoveStateAsync(installedAppId);
        }
    }
}
