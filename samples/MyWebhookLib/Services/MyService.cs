#region Copyright
// <copyright file="MyService.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Models;
using System;
using System.Threading.Tasks;

namespace MyWebhookLib.Services
{
    public interface IMyService
    {
        Task<dynamic> HandleRequestAsync(HttpRequest request);
        Task<MyState> GetStateAsync(string installedAppId);
        Task RemoveStateAsync(string installedAppId);
        Task DeviceCommandAsync(string installedAppId,
            string deviceId,
            dynamic command);
        Task LightSwitchCommandAsync(string installedAppId,
            string deviceId,
            bool toggle);
    }

    public class MyService : IMyService
    {
        private readonly ILogger<IMyService> logger;
        private readonly IRootWebhookHandler rootHandler;
        private readonly IStateManager<MyState> stateManager;
        private readonly IInstalledAppManager installedAppManager;
        private readonly ISmartThingsAPIHelper smartThingsAPIHelper;

        public MyService(ILogger<IMyService> logger,
            IRootWebhookHandler rootHandler,
            IStateManager<MyState> stateManager,
            IInstalledAppManager installedAppManager,
            ISmartThingsAPIHelper smartThingsAPIHelper)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = rootHandler ?? throw new ArgumentNullException(nameof(rootHandler));
            _ = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _ = installedAppManager ?? throw new ArgumentNullException(nameof(installedAppManager));
            _ = smartThingsAPIHelper ?? throw new ArgumentNullException(nameof(smartThingsAPIHelper));

            this.logger = logger;
            this.rootHandler = rootHandler;
            this.stateManager = stateManager;
            this.installedAppManager = installedAppManager;
            this.smartThingsAPIHelper = smartThingsAPIHelper;
        }

        public async Task<dynamic> HandleRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            try
            {
                return await rootHandler.HandleRequestAsync(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling rootHandler.HandleRequestAsync");
                throw;
            }
        }

        public async Task<MyState> GetStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            try
            {
                return await stateManager.GetStateAsync(installedAppId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling stateManager.GetStateAsync");
                throw;
            }
        }

        public async Task RemoveStateAsync(string installedAppId)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));

            try
            {
                await stateManager.RemoveStateAsync(installedAppId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling stateManager.GetStateAsync");
                throw;
            }
        }

        public async Task DeviceCommandAsync(string installedAppId,
            string deviceId,
            dynamic command)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));
            _ = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            _ = command ?? throw new ArgumentNullException(nameof(command));

            try
            {
                var installedApp = await installedAppManager.GetInstalledAppAsync(installedAppId).ConfigureAwait(false);
                await smartThingsAPIHelper.DeviceCommandAsync(installedApp, deviceId, command);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling smartThingsAPIHelper.DeviceCommandAsync");
                throw;
            }
        }

        public async Task LightSwitchCommandAsync(string installedAppId,
            string deviceId,
            bool toggle)
        {
            _ = installedAppId ?? throw new ArgumentNullException(nameof(installedAppId));
            _ = deviceId ?? throw new ArgumentNullException(nameof(deviceId));

            try
            {
                var installedApp = await installedAppManager.GetInstalledAppAsync(installedAppId).ConfigureAwait(false);
                var command = LightSwitch.GetDeviceCommand(toggle);
                await smartThingsAPIHelper.DeviceCommandAsync(installedApp, deviceId, command);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception calling smartThingsAPIHelper.DeviceCommandAsync");
                throw;
            }
        }
    }
}
