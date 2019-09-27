using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUninstallWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
    }

    public class UninstallWebhookHandler : IUninstallWebhookHandler
    {
        private ILogger<UninstallWebhookHandler> logger;

        public UninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            dynamic response = new JObject();
            response.uninstallData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
