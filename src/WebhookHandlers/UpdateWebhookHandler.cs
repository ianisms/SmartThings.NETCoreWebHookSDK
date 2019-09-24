using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public class UpdateWebhookHandler
    {
        private ILogger<UpdateWebhookHandler> logger;

        public UpdateWebhookHandler(ILogger<UpdateWebhookHandler> logger)
        {
            this.logger = logger;
        }

        public UpdateResponse HandleRequest(UpdateRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            logger.LogDebug($"{this.GetType().Name} handling request: {request.ToJson()}");

            var response = new UpdateResponse()
            {
                UpdateData = new UpdateResponseData()
            };

            logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
