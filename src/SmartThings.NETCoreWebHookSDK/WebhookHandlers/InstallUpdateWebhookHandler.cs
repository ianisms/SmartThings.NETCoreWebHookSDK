using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IInstallUpdateWebhookHandler
    {
        ILogger<IInstallUpdateWebhookHandler> logger { get; }
        IInstalledAppManager installedAppManager { get; }
        SmartAppConfig appConfig { get; }
        Task HandleUpdateDataAsync(InstalledApp installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);
        Task HandleInstallDataAsync(InstalledApp installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);
        Task<dynamic> HandleRequestAsync(Lifecycle lifecycle, dynamic request);
        dynamic ValidateRequest(Lifecycle lifecycle, dynamic request);
    }

    public abstract class InstallUpdateWebhookHandler : IInstallUpdateWebhookHandler
    {
        public ILogger<IInstallUpdateWebhookHandler> logger { get; private set; }
        public SmartAppConfig appConfig { get; private set; }
        public IInstalledAppManager installedAppManager { get; private set; }
        public ISmartThingsAPIHelper smartThingsAPIHelper { get; private set; }
        public HttpClient httpClient { get; private set; }

        public InstallUpdateWebhookHandler(ILogger<IInstallUpdateWebhookHandler> logger,
            IOptions<SmartAppConfig> options,
            IInstalledAppManager installedAppManager,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = options ??
                throw new ArgumentNullException(nameof(options));
            _ = installedAppManager ??
                throw new ArgumentNullException(nameof(installedAppManager));
            _ = smartThingsAPIHelper ??
                throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.logger = logger;
            this.appConfig = options.Value;
            this.installedAppManager = installedAppManager;
            this.smartThingsAPIHelper = smartThingsAPIHelper;
        }

        public virtual dynamic ValidateRequest(Lifecycle lifecycle, dynamic request)
        {
            logger.LogDebug($"validating request: {request}");

            dynamic dataToken = null;
            if (lifecycle == Lifecycle.Install)
            {
                dataToken = request.installData;
                _ = dataToken ??
                  throw new ArgumentException("request.installData is null", nameof(request));
            }
            else if (lifecycle == Lifecycle.Update)
            {
                dataToken = request.updateData;
                _ = dataToken ??
                  throw new ArgumentException("request.updateData is null", nameof(request));

            }

            _ = dataToken.authToken ??
                throw new ArgumentException("request.updateData.authToken is null", nameof(request));
            _ = dataToken.refreshToken ??
                throw new ArgumentException("request.updateData.refreshToken is null", nameof(request));
            _ = dataToken.installedApp ??
                throw new ArgumentException("request.updateData.installedApp is null", nameof(request));
            _ = dataToken.installedApp.installedAppId ??
                throw new ArgumentException("request.updateData.installedApp.installedAppId is null", nameof(request));
            _ = dataToken.installedApp.locationId ??
                throw new ArgumentException("request.updateData.installedApp.locationId is null", nameof(request));
            _ = dataToken.installedApp.config ??
                throw new ArgumentException("request.updateData.installedApp.config is null", nameof(request));

            return dataToken;
        }

        public abstract Task HandleUpdateDataAsync(InstalledApp installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);

        public abstract Task HandleInstallDataAsync(InstalledApp installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);

        public virtual async Task<dynamic> HandleRequestAsync(Lifecycle lifecycle,
            dynamic request)
        {
            dynamic dataToken = ValidateRequest(lifecycle, request);

            logger.LogInformation("Handling install/update request...");

            logger.LogTrace($"Handling request: {request}");

            var authToken = dataToken.authToken.Value;
            var refreshToken = dataToken.refreshToken.Value;
            var installedAppId = dataToken.installedApp.installedAppId.Value;
            var locationId = dataToken.installedApp.locationId.Value;

            InstalledApp installedApp = new InstalledApp()
            {
                InstalledAppId = installedAppId
            };

            logger.LogDebug("Setting tokens...");

            installedApp.SetTokens(authToken, refreshToken);

            logger.LogDebug("Setting location...");

            var location = await smartThingsAPIHelper.GetLocationAsync(
                installedApp,
                locationId);

            installedApp.InstalledLocation = location;

            logger.LogDebug("Storing installedApp...");

            await installedAppManager.StoreInstalledAppAsync(installedApp);

            if(lifecycle == Lifecycle.Install)
            {
                logger.LogDebug("HandleInstallDataAsync...");

                await HandleInstallDataAsync(installedApp, dataToken);
            }
            else if (lifecycle == Lifecycle.Update)
            {

                logger.LogDebug("Clearing subscriptions...");

                await smartThingsAPIHelper.ClearSubscriptionsAsync(installedApp);

                logger.LogDebug($"HandleUpdateDataAsync...");

                await HandleUpdateDataAsync(installedApp, dataToken);
            }
            else
            {
                throw new ArgumentException($"invalid lifecycle: {lifecycle}", nameof(lifecycle));
            }

            dynamic response = new JObject();
            response.updateData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
