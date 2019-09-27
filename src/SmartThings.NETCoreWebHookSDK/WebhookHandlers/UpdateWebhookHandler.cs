using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUpdateWebhookHandler
    {
        ILogger<UpdateWebhookHandler> logger { get; }
        dynamic HandleRequest(dynamic request);
        void ValidateRequest(dynamic request);
        void HandleUpdateData(dynamic updateData);
    }

    public abstract class UpdateWebhookHandler : IUpdateWebhookHandler
    {
        public ILogger<UpdateWebhookHandler> logger { get; private set; }

        public UpdateWebhookHandler(ILogger<UpdateWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public abstract void HandleUpdateData(dynamic updateData);

        public virtual void ValidateRequest(dynamic request)
        {
            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.updateData ??
                throw new InvalidOperationException("request.updateData is null");
            _ = request.updateData.authToken ??
                throw new InvalidOperationException("request.updateData.authToken is null");
            _ = request.updateData.refreshToken ??
                throw new InvalidOperationException("request.updateData.refreshToken is null");
            _ = request.updateData.installedApp ??
                throw new InvalidOperationException("request.updateData.installedApp is null");
            _ = request.updateData.installedApp.locationId ??
                throw new InvalidOperationException("request.updateData.installedApp.locationId is null");
            _ = request.updateData.installedApp.config ??
                throw new InvalidOperationException("request.updateData.installedApp.config is null");
        }

        public virtual dynamic HandleRequest(dynamic request)
        {
            ValidateRequest(request);

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            HandleUpdateData(request.updateData);

            dynamic response = new JObject();
            response.updateData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
