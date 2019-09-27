using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace AzureFunctionsApp.WebhookHandlers
{
    public class MyUninstallWebhookHandler : UninstallWebhookHandler
    {
        public MyUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger) : base(logger)
        {
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            // TODO: validate request.installData.installedApp.config
        }

        public override void HandleUninstallData(dynamic uninstallData)
        {
            // TODO: Subscribe to device events, etc.
        }
    }
}
