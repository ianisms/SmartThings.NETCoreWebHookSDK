using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ASPNetCoreWebAPI.WebhookHandlers
{
    public class MyInstallWebhookHandler : InstallWebhookHandler
    {
        public MyInstallWebhookHandler(ILogger<InstallWebhookHandler> logger) : base(logger)
        {
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            // TODO: validate request.installData.installedApp.config
        }

        public override void HandleInstallData(dynamic installData)
        {
            // TODO: Subscribe to device events, etc.
        }
    }
}
