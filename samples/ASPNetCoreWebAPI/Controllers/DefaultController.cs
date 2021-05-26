#region Copyright
// <copyright file="DefaultController.cs" company="Ian N. Bennett">
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// SmartThings.NETCoreWebHookSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SmartThings.NETCoreWebHookSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
#endregion

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
        private readonly ILogger<DefaultController> _logger;
        private readonly IMyService _myService;

        public DefaultController(ILogger<DefaultController> logger,
            IMyService myService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _myService = myService ??
                throw new ArgumentNullException(nameof(myService));
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
                var responseObj = await _myService.HandleRequestAsync(Request);
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
                _logger.LogError(ex, "Exception calling myService.HandleRequestAsync");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
