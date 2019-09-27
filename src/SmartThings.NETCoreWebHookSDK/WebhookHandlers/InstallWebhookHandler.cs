using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IInstallWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
    }

    public class InstallWebhookHandler : IInstallWebhookHandler
    {
        private ILogger<ConfigWebhookHandler> logger;

        public InstallWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            dynamic response = new JObject();
            response.installData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
