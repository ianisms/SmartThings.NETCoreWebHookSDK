using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IOAuthWebhookHandler
    {
        OAuthCallbackResponse HandleRequest(OAuthCallbackRequest request);
    }

    public class OAuthWebhookHandler : IOAuthWebhookHandler
    {
        private ILogger<OAuthWebhookHandler> logger;

        public OAuthWebhookHandler(ILogger<OAuthWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public OAuthCallbackResponse HandleRequest(OAuthCallbackRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            var response = new OAuthCallbackResponse()
            {
                OauthData = new OAuthCallbackResponseData()
            };

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
