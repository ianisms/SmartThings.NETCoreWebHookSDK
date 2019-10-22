#region Copyright
// <copyright file="MyUninstallWebhookHandler.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace MyWebhookLib.WebhookHandlers
{
    public class MyUninstallWebhookHandler : UninstallWebhookHandler
    {
        private readonly IStateManager<MyState> stateManager;

        public MyUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger,
            IInstalledAppManager installedAppManager,
            IStateManager<MyState> stateManager)
            : base(logger, installedAppManager)
        {
            _ = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            _ = request.uninstallData.installedApp.config.isAppEnabled ??
                throw new InvalidOperationException("request.uninstallData.installedApp.config.isAppEnabled is null");
            _ = request.uninstallData.installedApp.config.presenceSensors ??
                throw new InvalidOperationException("request.uninstallData.installedApp.config.presenceSensors is null");
        }

        public override async Task HandleUninstallDataAsync(dynamic uninstallData)
        {
            var installedAppId = uninstallData.installedApp.InstalledAppId;
            await stateManager.RemoveStateAsync(installedAppId);
        }
    }
}
