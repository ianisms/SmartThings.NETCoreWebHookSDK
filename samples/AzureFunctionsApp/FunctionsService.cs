using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Services;
using System;
using System.Threading.Tasks;

namespace AzureFunctionsApp
{
    public class FunctionsService
    {
        private readonly ILogger<FunctionsService> logger;
        private readonly IMyService myService;

        public FunctionsService(ILogger<FunctionsService> logger,
            IMyService myService)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = myService ??
                throw new ArgumentNullException(nameof(myService));

            this.logger = logger;
            this.myService = myService;
        }

        [FunctionName("FirstWH")]
        public async Task<IActionResult> FirstWH(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request)
        {
            try
            {
                var responseObj = await myService.HandleRequestAsync(request);
                if (responseObj != null)
                {
                    return new OkObjectResult(responseObj);
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling myService.HandleRequestAsync");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
