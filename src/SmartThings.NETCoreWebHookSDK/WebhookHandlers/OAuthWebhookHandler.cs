using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IOAuthWebhookHandler
    {
        ILogger<IOAuthWebhookHandler> logger { get; }
        Task<dynamic> HandleRequestAsync(dynamic request);
    }

    public class OAuthWebhookHandler : IOAuthWebhookHandler
    {
        public ILogger<IOAuthWebhookHandler> logger { get; private set; }

        public OAuthWebhookHandler(ILogger<IOAuthWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public async Task<dynamic> HandleRequestAsync(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"handling request: {request}");

            dynamic response = new JObject();
            response.oAuthCallbackData = new JObject();

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
