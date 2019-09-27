using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IConfigWebhookHandler
    {
        ILogger<ConfigWebhookHandler> logger { get; }

        dynamic HandleRequest(dynamic request);
        dynamic Initialize(dynamic request);
        dynamic Page(dynamic request);
    }

    public abstract class ConfigWebhookHandler : IConfigWebhookHandler
    {
        public ILogger<ConfigWebhookHandler> logger { get; private set; }

        public ConfigWebhookHandler(ILogger<ConfigWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            _ = request.configurationData ?? throw new InvalidOperationException("Missing configurationData!");
            _ = request.configurationData.phase ?? throw new InvalidOperationException("Missing configurationData.phase!");

            logger.LogDebug($"{this.GetType().Name} handling request: {request}");

            var phase = request.configurationData.phase.Value.ToLowerInvariant();

            dynamic response;

            if(phase == "initialize")
            {
                response = Initialize(request);
            }
            else
            {
                response = Page(request);
            }

            logger.LogDebug($"Response: {response}");

            return response;
        }

        public abstract dynamic Initialize(dynamic request);

        public abstract dynamic Page(dynamic request);
    }
}
