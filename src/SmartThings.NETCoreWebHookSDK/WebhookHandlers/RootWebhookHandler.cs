using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IRootWebhookHandler
    {
        Task<BaseResponse> HandleRequestAsync(HttpRequest request);
    }

    public class RootWebhookHandler : IRootWebhookHandler
    {
        private readonly ILogger<RootWebhookHandler> logger;
        private readonly IPingWebhookHandler pingHandler;
        private readonly IConfigWebhookHandler configHandler;
        private readonly IInstallWebhookHandler installHandler;
        private readonly IUpdateWebhookHandler updateHandler;
        private readonly IEventWebhookHandler eventHandler;
        private readonly IOAuthWebhookHandler oauthHandler;
        private readonly IUninstallWebhookHandler uninstallHandler;
        private readonly ICryptoUtils cryptoUtils;

        public RootWebhookHandler(ILogger<RootWebhookHandler> logger,
            IPingWebhookHandler pingHandler,
            IConfigWebhookHandler configHandler,
            IInstallWebhookHandler installHandler,
            IUpdateWebhookHandler updateHandler,
            IEventWebhookHandler eventHandler,
            IOAuthWebhookHandler oauthHandler,
            IUninstallWebhookHandler uninstallHandler,
            ICryptoUtils cryptoUtils)
        {
            this.logger = logger;
            this.pingHandler = pingHandler;
            this.configHandler = configHandler;
            this.installHandler = installHandler;
            this.updateHandler = updateHandler;
            this.eventHandler = eventHandler;
            this.oauthHandler = oauthHandler;
            this.uninstallHandler = uninstallHandler;
            this.cryptoUtils = cryptoUtils;
        }

        public async Task<BaseResponse> HandleRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            using (var reader = new StreamReader(request.Body))
            {
                var requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                RequestLifecycle lifeCycle;
                if (Enum.TryParse<RequestLifecycle>(data.lifecycle.Value, true, out lifeCycle))
                {
                    switch (lifeCycle)
                    {
                        case RequestLifecycle.Ping:
                            var pingRequest = PingRequest.FromJson(requestBody);
                            return pingHandler.HandleRequest(pingRequest);
                        case RequestLifecycle.Configuration:
                            await CheckAuthAsync(request).ConfigureAwait(false);
                            var configRequest = ConfigRequest.FromJson(requestBody);
                            return configHandler.HandleRequest(configRequest);
                        case RequestLifecycle.Install:
                            await CheckAuthAsync(request).ConfigureAwait(false);
                            var installRequest = InstallRequest.FromJson(requestBody);
                            return installHandler.HandleRequest(installRequest);
                        case RequestLifecycle.Update:
                            await CheckAuthAsync(request).ConfigureAwait(false);
                            var updateRequest = UpdateRequest.FromJson(requestBody);
                            return updateHandler.HandleRequest(updateRequest);
                        case RequestLifecycle.Event:
                            await CheckAuthAsync(request).ConfigureAwait(false);
                            var eventRequest = EventRequest.FromJson(requestBody);
                            return eventHandler.HandleRequest(eventRequest);
                        case RequestLifecycle.Uninstall:
                            await CheckAuthAsync(request).ConfigureAwait(false);
                            var uninstallRequest = UninstallRequest.FromJson(requestBody);
                            return uninstallHandler.HandleRequest(uninstallRequest);
                        case RequestLifecycle.OAuthCallback:
                            await CheckAuthAsync(request).ConfigureAwait(false);
                            var oauthRequest = OAuthCallbackRequest.FromJson(requestBody);
                            return oauthHandler.HandleRequest(oauthRequest);
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        private async Task CheckAuthAsync(HttpRequest request)
        {
            if (!await cryptoUtils.VerifySignedRequestAsync(request).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Could not verify request signature!");
            }
        }
    }
}
