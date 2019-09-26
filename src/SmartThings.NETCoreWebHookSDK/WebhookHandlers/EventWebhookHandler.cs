using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IEventWebhookHandler
    {
        EventResponse HandleRequest(EventRequest request);
    }

    public class EventWebhookHandler : IEventWebhookHandler
    {
        private ILogger<EventWebhookHandler> logger;

        public EventWebhookHandler(ILogger<EventWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public EventResponse HandleRequest(EventRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            var response = new EventResponse()
            {
                EventData = new EventResponseData()
            };

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
