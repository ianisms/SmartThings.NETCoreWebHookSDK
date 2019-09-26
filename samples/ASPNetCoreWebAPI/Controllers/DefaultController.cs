using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ASPNetCoreWebAPI.Controllers
{
    [Route("api/FirstWH")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly ILogger<DefaultController> logger;
        private readonly IRootWebhookHandler rootHandler;

        public DefaultController(ILogger<DefaultController> logger,
            IRootWebhookHandler rootHandler)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = rootHandler ?? throw new ArgumentNullException(nameof(rootHandler));

            this.logger = logger;
            this.rootHandler = rootHandler;
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
            var responseObj = await rootHandler.HandleRequestAsync(Request).ConfigureAwait(true);
            if (responseObj != null)
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
