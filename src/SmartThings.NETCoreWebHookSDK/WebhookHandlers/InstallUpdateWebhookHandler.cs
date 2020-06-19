#region Copyright
// <copyright file="InstallUpdateWebhookHandler.cs" company="Ian N. Bennett">
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
