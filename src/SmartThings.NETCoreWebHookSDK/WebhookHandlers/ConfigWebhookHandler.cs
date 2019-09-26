using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IConfigWebhookHandler
    {
        ILogger<ConfigWebhookHandler> Logger { get; }

        ConfigResponse HandleRequest(ConfigRequest request);
        ConfigResponse Initialize();
        ConfigResponse Page();
    }

    public abstract class ConfigWebhookHandler : IConfigWebhookHandler
    {
        public ILogger<ConfigWebhookHandler> Logger { get; private set; }

        public ConfigWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        {
            this.Logger = logger;
        }

        public ConfigResponse HandleRequest(ConfigRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            Logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            ConfigResponse configResp;

            if (request.ConfigurationData.Phase ==
                ConfigRequestData.RequestPhase.Initialize)
            {
                configResp = Initialize();
            }
            else
            {
                configResp = Page();
            }

            Logger.LogDebug($"Response: {configResp}");

            return configResp;
        }

        public abstract ConfigResponse Initialize();

        public abstract ConfigResponse Page();
    }
}
