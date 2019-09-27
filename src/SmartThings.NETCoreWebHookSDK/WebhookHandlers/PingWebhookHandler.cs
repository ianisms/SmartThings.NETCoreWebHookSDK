using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IPingWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
    }

    public class PingWebhookHandler : IPingWebhookHandler
    {
        private ILogger<ConfigWebhookHandler> logger;

        public PingWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            dynamic response = new JObject();
            response.pingData = request.pingData;

            logger.LogDebug($"response: {response}");
            return response;
        }
    }
}
