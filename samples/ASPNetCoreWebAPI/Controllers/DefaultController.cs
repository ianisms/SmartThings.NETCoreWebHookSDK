using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Services;
using System;
using System.Threading.Tasks;

namespace ASPNetCoreWebAPI.Controllers
{
    [Route("api/FirstWH")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly ILogger<DefaultController> logger;
        private readonly IMyService myService;

        public DefaultController(ILogger<DefaultController> logger,
            IMyService myService)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = myService ??
                throw new ArgumentNullException(nameof(myService));

            this.logger = logger;
            this.myService = myService;
        }


        // GET api/FirstWH
        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult("Test");
        }

        // POST api/FirstWH
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                var responseObj = await myService.HandleRequestAsync(Request);
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
