using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IEventWebhookHandler
    {
        dynamic HandleRequest(dynamic request);
        void ValidateRequest(dynamic request);
        void HandleEventData(dynamic eventData);
    }

    public abstract class EventWebhookHandler : IEventWebhookHandler
    {
        private ILogger<EventWebhookHandler> logger;

        public EventWebhookHandler(ILogger<EventWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public abstract void HandleEventData(dynamic eventData);

        public virtual void ValidateRequest(dynamic request)
        {
            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.eventData ??
                throw new InvalidOperationException("request.eventData is null");
            _ = request.eventData.authToken ??
                throw new InvalidOperationException("request.eventData.authToken is null");
            _ = request.eventData.installedApp ??
                throw new InvalidOperationException("request.eventData.installedApp is null");
            _ = request.eventData.installedApp.locationId ??
                throw new InvalidOperationException("request.eventData.installedApp.locationId is null");
            _ = request.eventData.installedApp.config ??
                throw new InvalidOperationException("request.eventData.installedApp.config is null");
            _ = request.eventData.events ??
                throw new InvalidOperationException("request.eventData.events is null");
        }

        public virtual dynamic HandleRequest(dynamic request)
        {
            ValidateRequest(request);

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            HandleEventData(request.eventData);

            dynamic response = new JObject();
            response.eventData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
