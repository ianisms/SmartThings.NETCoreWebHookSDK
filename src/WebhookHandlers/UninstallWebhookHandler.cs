using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public class UninstallWebhookHandler
    {
        private ILogger<UninstallWebhookHandler> logger;

        public UninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public UninstallResponse HandleRequest(UninstallRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            var response = new UninstallResponse()
            {
                UninstallData = new UninstallResponseData()
            };

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
