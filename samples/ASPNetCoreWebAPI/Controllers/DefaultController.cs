﻿using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
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
        private readonly PingWebhookHandler pingHandler;
        private readonly ASPNetCoreWebAPI.WebhookHandlers.ConfigWebhookHandler configHandler;
        private readonly InstallWebhookHandler installHandler;
        private readonly UpdateWebhookHandler updateHandler;
        private readonly EventWebhookHandler eventHandler;
        private readonly OAuthWebhookHandler oauthHandler;
        private readonly UninstallWebhookHandler uninstallHandler;

        public DefaultController(ILogger<DefaultController> logger,
            PingWebhookHandler pingHandler,
            ASPNetCoreWebAPI.WebhookHandlers.ConfigWebhookHandler configHandler,
            InstallWebhookHandler installHandler,
            UpdateWebhookHandler updateHandler,
            EventWebhookHandler eventHandler,
            OAuthWebhookHandler oauthHandler,
            UninstallWebhookHandler uninstallHandler)
        {
            this.logger = logger;
            this.pingHandler = pingHandler;
            this.configHandler = configHandler;
            this.installHandler = installHandler;
            this.updateHandler = updateHandler;
            this.eventHandler = eventHandler;
            this.oauthHandler = oauthHandler;
            this.uninstallHandler = uninstallHandler;
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
            using (var reader = new StreamReader(Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync().ConfigureAwait(true);
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                RequestLifecycle lifeCycle;
                if (Enum.TryParse<RequestLifecycle>(data.lifecycle.Value, true, out lifeCycle))
                {
                    switch (lifeCycle)
                    {
                        case RequestLifecycle.Ping:
                            var pingRequest = PingRequest.FromJson(requestBody);
                            return new OkObjectResult(pingHandler.HandleRequest(pingRequest));
                        case RequestLifecycle.Configuration:
                            var configRequest = ConfigRequest.FromJson(requestBody);
                            return new OkObjectResult(configHandler.HandleRequest(configRequest));
                        case RequestLifecycle.Install:
                            var installRequest = InstallRequest.FromJson(requestBody);
                            return new OkObjectResult(installHandler.HandleRequest(installRequest));
                        case RequestLifecycle.Update:
                            var updateRequest = UpdateRequest.FromJson(requestBody);
                            return new OkObjectResult(updateHandler.HandleRequest(updateRequest));
                        case RequestLifecycle.Event:
                            var eventRequest = EventRequest.FromJson(requestBody);
                            return new OkObjectResult(eventHandler.HandleRequest(eventRequest));
                        case RequestLifecycle.Uninstall:
                            var uninstallRequest = UninstallRequest.FromJson(requestBody);
                            return new OkObjectResult(uninstallHandler.HandleRequest(uninstallRequest));
                        case RequestLifecycle.OAuthCallback:
                            var oauthRequest = OAuthCallbackRequest.FromJson(requestBody);
                            return new OkObjectResult(oauthHandler.HandleRequest(oauthRequest));
                        default:
                            break;
                    }
                }
            }

            throw new InvalidOperationException();
        }
    }
}
