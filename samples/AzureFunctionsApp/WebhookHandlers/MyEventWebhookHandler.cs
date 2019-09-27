using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace AzureFunctionsApp.WebhookHandlers
{
    public class MyEventWebhookHandler : EventWebhookHandler
    {
        public MyEventWebhookHandler(ILogger<EventWebhookHandler> logger) : base(logger)
        {
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            // TODO: validate request.eventData
        }

        public override void HandleEventData(dynamic eventData)
        {
            // TODO: Subscribe to device events, etc.
        }
    }
}
