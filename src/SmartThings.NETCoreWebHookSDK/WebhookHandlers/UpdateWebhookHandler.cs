using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUpdateWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
    }

    public class UpdateWebhookHandler : IUpdateWebhookHandler
    {
        private ILogger<UpdateWebhookHandler> logger;

        public UpdateWebhookHandler(ILogger<UpdateWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            dynamic response = new JObject();
            response.updateData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
