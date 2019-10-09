using ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUninstallWebhookHandler
    {
        ILogger<IUninstallWebhookHandler> logger { get; }
        IInstalledAppManager installedAppManager { get; }
        Task<dynamic> HandleRequestAsync(dynamic request);
        void ValidateRequest(dynamic request);
        Task HandleUninstallDataAsync(dynamic uninstallData);
    }

    public abstract class UninstallWebhookHandler : IUninstallWebhookHandler
    {
        public ILogger<IUninstallWebhookHandler> logger { get; private set; }
        public IInstalledAppManager installedAppManager { get; private set; }

        public UninstallWebhookHandler(ILogger<IUninstallWebhookHandler> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ?? throw new ArgumentNullException(nameof(installedAppManager));

            this.logger = logger;
            this.installedAppManager = installedAppManager;
        }

        public abstract Task HandleUninstallDataAsync(dynamic uninstallData);

        public virtual void ValidateRequest(dynamic request)
        {
            logger.LogTrace($"Validating request: {request}");

            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.uninstallData ??
                throw new ArgumentException("request.uninstallData is null",
                    nameof(request));
            _ = request.uninstallData.authToken ??
                throw new ArgumentException("request.uninstallData.authToken is null",
                    nameof(request));
            _ = request.uninstallData.installedApp ??
                throw new ArgumentException("request.uninstallData.installedApp is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.installedAppId ??
                throw new ArgumentException("request.uninstallData.installedApp.installedAppId is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.locationId ??
                throw new ArgumentException("request.uninstallData.installedApp.locationId is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.config ??
                throw new ArgumentException("request.uninstallData.installedApp.config is null",
                    nameof(request));
            _ = request.uninstallData.events ??
                throw new ArgumentException("request.uninstallData.events is null",
                    nameof(request));
        }

        public virtual async Task<dynamic> HandleRequestAsync(dynamic request)
        {
            ValidateRequest(request);

            logger.LogTrace($"Handling request: {request}");

            await HandleUninstallDataAsync(request.uninstallData);

            var installedAppId = request.uninstallData.installedApp.installedAppId;

            await installedAppManager.RemoveInstalledAppAsync(installedAppId);

            dynamic response = new JObject();
            response.uninstallData = new JObject();

            logger.LogTrace($"Response: {response}");

            return response;
        }
    }
}
