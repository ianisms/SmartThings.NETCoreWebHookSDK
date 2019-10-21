#region Copyright
// <copyright file="EventWebhookHandler.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IEventWebhookHandler
    {
        ILogger<IEventWebhookHandler> logger { get; }
        IInstalledAppManager installedAppManager { get; }
        Task<dynamic> HandleRequestAsync(dynamic request);
        void ValidateRequest(dynamic request);
        Task HandleEventDataAsync(InstalledApp installedApp, dynamic eventData);
    }

    public abstract class EventWebhookHandler : IEventWebhookHandler
    {
        public ILogger<IEventWebhookHandler> logger { get; private set; }
        public IInstalledAppManager installedAppManager { get; private set; }

        public EventWebhookHandler(ILogger<IEventWebhookHandler> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ??
                throw new ArgumentNullException(nameof(installedAppManager));

            this.logger = logger;
            this.installedAppManager = installedAppManager;
        }

        public abstract Task HandleEventDataAsync(InstalledApp installedApp, dynamic eventData);

        public virtual void ValidateRequest(dynamic request)
        {
            logger.LogTrace($"Validating request: {request}");

            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.eventData ??
                throw new ArgumentException("request.eventData is null",
                    nameof(request));
            _ = request.eventData.authToken ??
                throw new ArgumentException("request.eventData.authToken is null",
                    nameof(request));
            _ = request.eventData.installedApp ??
                throw new ArgumentException("request.eventData.installedApp is null",
                    nameof(request));
            _ = request.eventData.installedApp.installedAppId ??
                throw new ArgumentException("request.eventData.installedApp.installedAppId is null",
                    nameof(request));
            _ = request.eventData.installedApp.locationId ??
                throw new ArgumentException("request.eventData.installedApp.locationId is null",
                    nameof(request));
            _ = request.eventData.installedApp.config ??
                throw new ArgumentException("request.eventData.installedApp.config is null",
                    nameof(request));
            _ = request.eventData.events ??
                throw new ArgumentException("request.eventData.events is null",
                    nameof(request));
        }

        public virtual async Task<dynamic> HandleRequestAsync(dynamic request)
        {
            ValidateRequest(request);

            logger.LogDebug("Handling event request...");
            logger.LogTrace($"Handling request: {request}");

            var installedAppId = request.eventData.installedApp.installedAppId.Value;

            logger.LogDebug($"Getting installed app for installedAppId: {installedAppId}...");

            var installedApp = await installedAppManager.GetInstalledAppAsync(installedAppId);

            if (installedApp == null)
            {
                throw new InvalidOperationException($"unable to retrive installed app for installedAppId: {installedAppId}");
            }

            logger.LogDebug("Setting tokens...");

            installedApp.SetTokens(request.eventData.authToken.Value);

            Task.Run(() => HandleEventDataAsync(installedApp, request.eventData));

            dynamic response = new JObject();
            response.eventData = new JObject();

            logger.LogTrace($"Response: {response}");

            return response;
        }
    }
}
