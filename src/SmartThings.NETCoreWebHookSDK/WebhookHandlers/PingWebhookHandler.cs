using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IPingWebhookHandler
    {
        ILogger<IPingWebhookHandler> logger { get; }
        dynamic HandleRequest(dynamic request);
    }

    public class PingWebhookHandler : IPingWebhookHandler
    {
        public ILogger<IPingWebhookHandler> logger { get; private set; }

        public PingWebhookHandler(ILogger<IPingWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public dynamic HandleRequest(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"handling request: {request}");

            dynamic response = new JObject();
            response.pingData = request.pingData;

            logger.LogDebug($"response: {response}");
            return response;
        }
    }
}
