using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IEventWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
    }

    public class EventWebhookHandler : IEventWebhookHandler
    {
        private ILogger<EventWebhookHandler> logger;

        public EventWebhookHandler(ILogger<EventWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            dynamic response = new JObject();
            response.eventData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
