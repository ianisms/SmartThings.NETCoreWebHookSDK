#region Copyright
// <copyright file="EventWebhookHandler.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
        ILogger<IEventWebhookHandler> Logger { get; }
        IInstalledAppManager InstalledAppManager { get; }
        Task<dynamic> HandleRequestAsync(dynamic request);
        void ValidateRequest(dynamic request);
        Task HandleEventDataAsync(InstalledAppInstance installedApp, dynamic eventData);
    }

    public abstract class EventWebhookHandler : IEventWebhookHandler
    {
        public ILogger<IEventWebhookHandler> Logger { get; private set; }
        public IInstalledAppManager InstalledAppManager { get; private set; }

        protected EventWebhookHandler(ILogger<IEventWebhookHandler> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ??
                throw new ArgumentNullException(nameof(installedAppManager));

            this.Logger = logger;
            this.InstalledAppManager = installedAppManager;
        }

        public abstract Task HandleEventDataAsync(InstalledAppInstance installedApp, dynamic eventData);

        public virtual void ValidateRequest(dynamic request)
        {
            Logger.LogTrace($"Validating request: {request}");

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

            Logger.LogDebug("Handling event request...");
            Logger.LogTrace($"Handling request: {request}");

            var installedAppId = request.eventData.installedApp.installedAppId.Value;

            Logger.LogDebug($"Getting installed app for installedAppId: {installedAppId}...");

            var installedApp = await InstalledAppManager.GetInstalledAppAsync(installedAppId);

            if (installedApp == null)
            {
                throw new InvalidOperationException($"unable to retrive installed app for installedAppId: {installedAppId}");
            }

            Logger.LogDebug("Setting tokens...");

            installedApp.SetTokens(request.eventData.authToken.Value);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => HandleEventDataAsync(installedApp, request.eventData));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            dynamic response = new JObject();
            response.eventData = new JObject();

            Logger.LogTrace($"Response: {response}");

            return response;
        }
    }
}
