using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IOAuthWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
    }

    public class OAuthWebhookHandler : IOAuthWebhookHandler
    {
        private ILogger<OAuthWebhookHandler> logger;

        public OAuthWebhookHandler(ILogger<OAuthWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            dynamic response = new JObject();
            response.oAuthCallbackData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
