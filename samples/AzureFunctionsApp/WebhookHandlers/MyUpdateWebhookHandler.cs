using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace AzureFunctionsApp.WebhookHandlers
{
    public class MyUpdateWebhookHandler : UpdateWebhookHandler
    {
        public MyUpdateWebhookHandler(ILogger<UpdateWebhookHandler> logger) : base(logger)
        {
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            // TODO: validate request.updateData.installedApp.config
        }

        public override void HandleUpdateData(dynamic updateData)
        {
            // TODO: Subscribe to device events, etc.
        }
    }
}
