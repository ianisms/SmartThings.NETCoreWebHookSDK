#region Copyright
// <copyright file="UninstallWebhookHandler.cs" company="Ian N. Bennett">
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUninstallWebhookHandler
    {
        ILogger<IUninstallWebhookHandler> Logger { get; }
        IInstalledAppManager InstalledAppManager { get; }
        Task<dynamic> HandleRequestAsync(dynamic request);
        void ValidateRequest(dynamic request);
        Task HandleUninstallDataAsync(dynamic uninstallData);
    }

    public abstract class UninstallWebhookHandler : IUninstallWebhookHandler
    {
        public ILogger<IUninstallWebhookHandler> Logger { get; private set; }
        public IInstalledAppManager InstalledAppManager { get; private set; }

        public UninstallWebhookHandler(ILogger<IUninstallWebhookHandler> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ?? throw new ArgumentNullException(nameof(installedAppManager));

            this.Logger = logger;
            this.InstalledAppManager = installedAppManager;
        }

        public abstract Task HandleUninstallDataAsync(dynamic uninstallData);

        public virtual void ValidateRequest(dynamic request)
        {
            Logger.LogTrace($"Validating request: {request}");

            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.uninstallData ??
                throw new ArgumentException("request.uninstallData is null",
                    nameof(request));
            _ = request.uninstallData.authToken ??
                throw new ArgumentException("request.uninstallData.authToken is null",
                    nameof(request));
            _ = request.uninstallData.installedApp ??
                throw new ArgumentException("request.uninstallData.installedApp is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.installedAppId ??
                throw new ArgumentException("request.uninstallData.installedApp.installedAppId is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.locationId ??
                throw new ArgumentException("request.uninstallData.installedApp.locationId is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.config ??
                throw new ArgumentException("request.uninstallData.installedApp.config is null",
                    nameof(request));
            _ = request.uninstallData.events ??
                throw new ArgumentException("request.uninstallData.events is null",
                    nameof(request));
        }

        public virtual async Task<dynamic> HandleRequestAsync(dynamic request)
        {
            ValidateRequest(request);

            Logger.LogTrace($"Handling request: {request}");

            await HandleUninstallDataAsync(request.uninstallData);

            var installedAppId = request.uninstallData.installedApp.installedAppId;

            await InstalledAppManager.RemoveInstalledAppAsync(installedAppId);

            dynamic response = new JObject();

            response.uninstallData = new JObject();

            Logger.LogTrace($"Response: {response}");

            return response;
        }
    }
}
