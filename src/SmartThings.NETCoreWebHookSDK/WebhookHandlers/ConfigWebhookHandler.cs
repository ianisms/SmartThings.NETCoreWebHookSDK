using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IConfigWebhookHandler
    {
        ILogger<IConfigWebhookHandler> logger { get; }
        dynamic HandleRequest(dynamic request);
        dynamic Initialize(dynamic request);
        dynamic Page(dynamic request);
    }

    public abstract class ConfigWebhookHandler : IConfigWebhookHandler
    {
        public ILogger<IConfigWebhookHandler> logger { get; private set; }

        public ConfigWebhookHandler(ILogger<IConfigWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public virtual void ValidateRequest(dynamic request)
        {
            logger.LogTrace($"Validating request: {request}");

            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.configurationData ??
                throw new InvalidOperationException("Missing configurationData!");
            _ = request.configurationData.phase ??
                throw new InvalidOperationException("Missing configurationData.phase!");
        }

        public dynamic HandleRequest(dynamic request)
        {
            ValidateRequest(request);

            logger.LogDebug("Handling config request...");
            logger.LogTrace($"Handling request: {request}");

            var phase = request.configurationData.phase.Value.ToLowerInvariant();

            logger.LogDebug($"Config phase: {phase}");

            dynamic response;

            if(phase == "initialize")
            {
                response = Initialize(request);
            }
            else
            {
                _ = request.configurationData.pageId ??
                    throw new InvalidOperationException("request.configurationData.pageId is null!");
                _ = request.configurationData.pageId.Value ??
                    throw new InvalidOperationException("request.configurationData.pageId is null!");

                response = Page(request);
            }

            logger.LogTrace($"Response: {response}");

            return response;
        }

        public abstract dynamic Initialize(dynamic request);

        public abstract dynamic Page(dynamic request);
    }
}
