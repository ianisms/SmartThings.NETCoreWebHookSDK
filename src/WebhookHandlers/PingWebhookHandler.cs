using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public class PingWebhookHandler
    {
        private ILogger<ConfigWebhookHandler> logger;

        public PingWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public PingResponse HandleRequest(PingRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            var response = PingResponse.FromPingRequest(request);

            logger.LogDebug($"response: {response}");
            return response;
        }
    }
}
