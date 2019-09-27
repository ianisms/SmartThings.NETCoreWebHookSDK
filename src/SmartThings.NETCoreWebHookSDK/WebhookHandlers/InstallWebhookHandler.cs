using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IInstallWebhookHandler
    {
        ILogger<InstallWebhookHandler> logger { get; }
        dynamic HandleRequest(dynamic request);
        void ValidateRequest(dynamic request);
        void HandleInstallData(dynamic installData);
    }

    public abstract class InstallWebhookHandler : IInstallWebhookHandler
    {
        public ILogger<InstallWebhookHandler> logger { get; private set; }

        public InstallWebhookHandler(ILogger<InstallWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public abstract void HandleInstallData(dynamic installData);

        public virtual void ValidateRequest(dynamic request)
        {
            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.installData ??
                throw new InvalidOperationException("request.installData is null");
            _ = request.installData.authToken ??
                throw new InvalidOperationException("request.installData.authToken is null");
            _ = request.installData.refreshToken ??
                throw new InvalidOperationException("request.installData.refreshToken is null");
            _ = request.installData.installedApp ??
                throw new InvalidOperationException("request.installData.installedApp is null");
            _ = request.installData.installedApp.locationId ??
                throw new InvalidOperationException("request.installData.installedApp.locationId is null");
            _ = request.installData.installedApp.config ??
                throw new InvalidOperationException("request.installData.installedApp.config is null");
        }

        public virtual dynamic HandleRequest(dynamic request)
        {
            ValidateRequest(request);

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            HandleInstallData(request.installData);

            dynamic response = new JObject();
            response.installData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
