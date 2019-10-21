using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IRootWebhookHandler
    {
        ILogger<IRootWebhookHandler> logger { get; }
        Task<dynamic> HandleRequestAsync(HttpRequest request);
    }

    public class RootWebhookHandler : IRootWebhookHandler
    {
        public ILogger<IRootWebhookHandler> logger { get; private set; }
        private readonly IPingWebhookHandler pingHandler;
        private readonly IConfigWebhookHandler configHandler;
        private readonly IInstallUpdateWebhookHandler installUpdateHandler;
        private readonly IEventWebhookHandler eventHandler;
        private readonly IOAuthWebhookHandler oauthHandler;
        private readonly IUninstallWebhookHandler uninstallHandler;
        private readonly ICryptoUtils cryptoUtils;

        public RootWebhookHandler(ILogger<IRootWebhookHandler> logger,
            IPingWebhookHandler pingHandler,
            IConfigWebhookHandler configHandler,
            IInstallUpdateWebhookHandler installUpdateHandler,
            IEventWebhookHandler eventHandler,
            IOAuthWebhookHandler oauthHandler,
            IUninstallWebhookHandler uninstallHandler,
            ICryptoUtils cryptoUtils)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = pingHandler ??
                throw new ArgumentNullException(nameof(pingHandler));
            _ = configHandler ??
                throw new ArgumentNullException(nameof(configHandler));
            _ = installUpdateHandler ??
                throw new ArgumentNullException(nameof(installUpdateHandler));
            _ = eventHandler ??
                throw new ArgumentNullException(nameof(eventHandler));
            _ = oauthHandler ??
                throw new ArgumentNullException(nameof(oauthHandler));
            _ = uninstallHandler ??
                throw new ArgumentNullException(nameof(uninstallHandler));
            _ = cryptoUtils ??
                throw new ArgumentNullException(nameof(cryptoUtils));

            this.logger = logger;
            this.pingHandler = pingHandler;
            this.configHandler = configHandler;
            this.installUpdateHandler = installUpdateHandler;
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
                var requestBody = await reader.ReadToEndAsync();
                dynamic data = JObject.Parse(requestBody);

                _ = data.lifecycle ??
                    throw new ArgumentException("lifecycle missing from request body!", nameof(request));
                _ = data.lifecycle.Value ??
                    throw new ArgumentException("lifecycle missing from request body!", nameof(request));

                var lifecycleVal = data.lifecycle.Value.ToLowerInvariant().Replace("_", "");

                Lifecycle lifecycle = Lifecycle.Unknown;
                if(!Enum.TryParse<Lifecycle>(lifecycleVal, true, out lifecycle))
                {
                    throw new ArgumentException($"data.lifecycle is an invalid value: {data.lifecycle.Value}", nameof(request));
                }

                switch (lifecycle)
                {
                    case Lifecycle.Ping:
                        return pingHandler.HandleRequest(data);
                    case Lifecycle.Configuration:
                        await CheckAuthAsync(request);
                        return configHandler.HandleRequest(data);
                    case Lifecycle.Install:
                        await CheckAuthAsync(request);
                        return await installUpdateHandler.HandleRequestAsync(lifecycle, data);
                    case Lifecycle.Update:
                        await CheckAuthAsync(request);
                        return await installUpdateHandler.HandleRequestAsync(lifecycle, data);
                    case Lifecycle.Event:
                        await CheckAuthAsync(request);
                        return await eventHandler.HandleRequestAsync(data);
                    case Lifecycle.Uninstall:
                        await CheckAuthAsync(request);
                        return await uninstallHandler.HandleRequestAsync(data);
                    case Lifecycle.Oauthcallback:
                        await CheckAuthAsync(request);
                        return await oauthHandler.HandleRequestAsync(data);
                    default:
                        break;
                }
            }

            return null;
        }

        private async Task CheckAuthAsync(HttpRequest request)
        {
            if (!await cryptoUtils.VerifySignedRequestAsync(request))
            {
                throw new InvalidOperationException("Could not verify request signature!");
            }
        }
    }
}
