#region Copyright
// <copyright file="InstallUpdateWebhookHandler.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IInstallUpdateWebhookHandler
    {
        ILogger<IInstallUpdateWebhookHandler> Logger { get; }
        IInstalledAppManager InstalledAppManager { get; }
        SmartAppConfig AppConfig { get; }
        Task HandleUpdateDataAsync(InstalledAppInstance installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);
        Task HandleInstallDataAsync(InstalledAppInstance installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);
        Task<dynamic> HandleRequestAsync(Lifecycle lifecycle, dynamic request);
        dynamic ValidateRequest(Lifecycle lifecycle, dynamic request);
    }

    public abstract class InstallUpdateWebhookHandler : IInstallUpdateWebhookHandler
    {
        public ILogger<IInstallUpdateWebhookHandler> Logger { get; private set; }
        public SmartAppConfig AppConfig { get; private set; }
        public IInstalledAppManager InstalledAppManager { get; private set; }
        public ISmartThingsAPIHelper SmartThingsAPIHelper { get; private set; }
        public HttpClient HttpClient { get; private set; }

        public InstallUpdateWebhookHandler(ILogger<IInstallUpdateWebhookHandler> logger,
            IOptions<SmartAppConfig> options,
            IInstalledAppManager installedAppManager,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = options ??
                throw new ArgumentNullException(nameof(options));
            _ = installedAppManager ??
                throw new ArgumentNullException(nameof(installedAppManager));
            _ = smartThingsAPIHelper ??
                throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.Logger = logger;
            this.AppConfig = options.Value;
            this.InstalledAppManager = installedAppManager;
            this.SmartThingsAPIHelper = smartThingsAPIHelper;
        }

        public virtual dynamic ValidateRequest(Lifecycle lifecycle, dynamic request)
        {
            Logger.LogDebug($"validating request: {request}");

            dynamic dataToken = null;
            if (lifecycle == Lifecycle.Install)
            {
                dataToken = request.installData;
                _ = dataToken ??
                  throw new ArgumentException("request.installData is null",
                  nameof(request));
            }
            else if (lifecycle == Lifecycle.Update)
            {
                dataToken = request.updateData;
                _ = dataToken ??
                  throw new ArgumentException("request.updateData is null",
                  nameof(request));

            }

            _ = dataToken.authToken ??
                throw new ArgumentException("request.updateData.authToken is null",
                nameof(request));
            _ = dataToken.refreshToken ??
                throw new ArgumentException("request.updateData.refreshToken is null",
                nameof(request));
            _ = dataToken.installedApp ??
                throw new ArgumentException("request.updateData.installedApp is null",
                nameof(request));
            _ = dataToken.installedApp.installedAppId ??
                throw new ArgumentException("request.updateData.installedApp.installedAppId is null",
                nameof(request));
            _ = dataToken.installedApp.locationId ??
                throw new ArgumentException("request.updateData.installedApp.locationId is null",
                nameof(request));
            _ = dataToken.installedApp.config ??
                throw new ArgumentException("request.updateData.installedApp.config is null",
                nameof(request));

            return dataToken;
        }

        public abstract Task HandleUpdateDataAsync(InstalledAppInstance installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);

        public abstract Task HandleInstallDataAsync(InstalledAppInstance installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true);

        public virtual async Task<dynamic> HandleRequestAsync(Lifecycle lifecycle,
            dynamic request)
        {
            dynamic dataToken = ValidateRequest(lifecycle, request);

            Logger.LogDebug("Handling install/update request...");

            Logger.LogTrace($"Handling request: {request}");

            var authToken = dataToken.authToken.Value;
            var refreshToken = dataToken.refreshToken.Value;
            var installedAppId = dataToken.installedApp.installedAppId.Value;
            var locationId = dataToken.installedApp.locationId.Value;

            InstalledAppInstance installedApp = new InstalledAppInstance()
            {
                InstalledAppId = installedAppId
            };

            Logger.LogDebug("Setting tokens...");

            installedApp.SetTokens(authToken, refreshToken);

            Logger.LogDebug("Setting location...");

            var location = await SmartThingsAPIHelper.GetLocationAsync(
                installedApp,
                locationId);

            installedApp.InstalledLocation = location;

            Logger.LogDebug("Storing installedApp...");

            await InstalledAppManager.StoreInstalledAppAsync(installedApp).ConfigureAwait(false);

            if (lifecycle == Lifecycle.Install)
            {
                Logger.LogDebug("HandleInstallDataAsync...");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => HandleInstallDataAsync(installedApp, dataToken).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else if (lifecycle == Lifecycle.Update)
            {

                Logger.LogDebug("Clearing subscriptions...");

                await SmartThingsAPIHelper.ClearSubscriptionsAsync(installedApp).ConfigureAwait(false);

                Logger.LogDebug($"HandleUpdateDataAsync...");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => HandleUpdateDataAsync(installedApp, dataToken).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                throw new ArgumentException($"invalid lifecycle: {lifecycle}",
                    nameof(lifecycle));
            }

            dynamic response = new JObject();
            response.updateData = new JObject();

            Logger.LogDebug($"response: {response}");

            return response;
        }
    }
}
