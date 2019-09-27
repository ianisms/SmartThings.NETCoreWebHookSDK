using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUninstallWebhookHandler
    {
        ILogger<UninstallWebhookHandler> logger { get; }
        dynamic HandleRequest(dynamic request);
        void ValidateRequest(dynamic request);
        void HandleUninstallData(dynamic uninstallData);
    }

    public abstract class UninstallWebhookHandler : IUninstallWebhookHandler
    {
        public ILogger<UninstallWebhookHandler> logger { get; private set; }

        public UninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public abstract void HandleUninstallData(dynamic uninstallData);

        public virtual void ValidateRequest(dynamic request)
        {
            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.uninstallData ??
                throw new InvalidOperationException("request.uninstallData is null");
           _ = request.uninstallData.installedApp ??
                throw new InvalidOperationException("request.uninstallData.installedApp is null");
            _ = request.uninstallData.installedApp.locationId ??
                throw new InvalidOperationException("request.uninstallData.installedApp.locationId is null");
            _ = request.uninstallData.installedApp.config ??
                throw new InvalidOperationException("request.uninstallData.installedApp.config is null");
        }

        public virtual dynamic HandleRequest(dynamic request)
        {
            ValidateRequest(request);

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            HandleUninstallData(request.uninstallData);

            dynamic response = new JObject();
            response.uninstallData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
