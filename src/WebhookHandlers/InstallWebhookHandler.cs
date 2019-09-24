using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public class InstallWebhookHandler
    {
        private ILogger<ConfigWebhookHandler> logger;

        public InstallWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public InstallResponse HandleRequest(InstallRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            var response = new InstallResponse()
            {
                InstallData = new InstallResponseData()
            };

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
