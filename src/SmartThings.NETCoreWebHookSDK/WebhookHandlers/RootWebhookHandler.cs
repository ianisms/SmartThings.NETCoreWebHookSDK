using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
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
        Task<dynamic> HandleRequestAsync(HttpRequest request);
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

        public async Task<dynamic> HandleRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            using (var reader = new StreamReader(request.Body))
            {
                var requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                _ = data.lifecycle ?? throw new InvalidOperationException("lifecycle missing from request body!");
                _ = data.lifecycle.Value ?? throw new InvalidOperationException("lifecycle missing from request body!");

                var lifecycle = data.lifecycle.Value.ToLowerInvariant().Replace("_", "");
                switch (lifecycle)
                {
                    case "ping":
                        return pingHandler.HandleRequest(data);
                    case "configuration":
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return configHandler.HandleRequest(data);
                    case "install":
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return installHandler.HandleRequest(data);
                    case "update":
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return updateHandler.HandleRequest(data);
                    case "event":
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return eventHandler.HandleRequest(data);
                    case "uninstall":
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return uninstallHandler.HandleRequest(data);
                    case "oauthcallback":
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return oauthHandler.HandleRequest(data);
                    default:
                        break;
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
