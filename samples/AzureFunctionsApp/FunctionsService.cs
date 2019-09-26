using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctionsApp
{
    public class FunctionsService
    {
        private readonly ILogger<FunctionsService> logger;
        private readonly IRootWebhookHandler rootHandler;

        public FunctionsService(ILogger<FunctionsService> logger,
            IRootWebhookHandler rootHandler)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = rootHandler ?? throw new ArgumentNullException(nameof(rootHandler));

            this.logger = logger;
            this.rootHandler = rootHandler;
        }

        [FunctionName("FirstWH")]
        public async Task<IActionResult> FirstWH(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            var responseObj = await rootHandler.HandleRequestAsync(request).ConfigureAwait(true);
            if(responseObj != null)
            {
                return new OkObjectResult(responseObj);
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
