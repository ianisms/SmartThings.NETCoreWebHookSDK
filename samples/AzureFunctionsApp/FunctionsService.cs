#region Copyright
// <copyright file="FunctionsService.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
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

using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
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
        private readonly IInstalledAppTokenManager installedAppTokenManager;
        private readonly IMyService myService;

        public FunctionsService(ILogger<FunctionsService> logger,
            IInstalledAppTokenManager installedAppTokenManager,
            IMyService myService)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = installedAppTokenManager ??
                throw new ArgumentNullException(nameof(installedAppTokenManager));
            _ = myService ??
                throw new ArgumentNullException(nameof(myService));

            this.logger = logger;
            this.installedAppTokenManager = installedAppTokenManager;
            this.myService = myService;
        }

        [FunctionName("FirstWH")]
        public async Task<IActionResult> FirstWH(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request)
        {
            try
            {
                var responseObj = await myService.HandleRequestAsync(request).ConfigureAwait(false);
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
                throw;
            }
        }

        /**  
         *  For functions apps, we cannot add a hosted service to handle token refresh so we simply
         *  spin up a timer trigger to periodically refresh the tokens.
         *  The schedule expression is for the current refreshToken timeout (30 minutes) with a bit of buffer.
         *  The accessToken will be refreshed before each api request.
         **/
        [FunctionName("InstalledAppTokenRefresh")]
        public async Task InstalledAppTokenRefresh(
            [TimerTrigger("0 */29 * * * *")] TimerInfo timer)
        {
            try
            {
                if (timer.IsPastDue)
                {
                    logger.LogDebug("Timer is running late!");
                }

                await installedAppTokenManager.RefreshAllTokensAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling installedAppTokenManager.RefreshAllTokensAsync");
                throw;
            }
        }
    }
}
