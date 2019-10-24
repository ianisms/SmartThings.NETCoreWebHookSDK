#region Copyright
// <copyright file="MyInstallUpdateDataHandler.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyWebhookLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebhookLib.WebhookHandlers
{
    public class MyInstallUpdateDataHandler : InstallUpdateWebhookHandler
    {
        private readonly IStateManager<MyState> stateManager;

        public MyInstallUpdateDataHandler(ILogger<IInstallUpdateWebhookHandler> logger,
            IOptions<SmartAppConfig> options,
            IInstalledAppManager installedAppManager,
            ISmartThingsAPIHelper smartThingsAPIHelper,
            IStateManager<MyState> stateManager)
            : base(logger, options, installedAppManager, smartThingsAPIHelper)
        {
            _ = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            this.stateManager = stateManager;
        }

        private async Task SubscribeToDeviceEventAsync(InstalledAppInstance installedApp,
            dynamic deviceConfig)
        {
            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));
            _ = deviceConfig ??
                throw new ArgumentNullException(nameof(deviceConfig));

            var resp = await SmartThingsAPIHelper.SubscribeToDeviceEventAsync(
                installedApp,
                deviceConfig);

            var body = await resp.Content.ReadAsStringAsync();
            dynamic subscriptionResp = JObject.Parse(body);

            _ = subscriptionResp.id ??
                throw new InvalidOperationException("subscriptionResp.id is null!");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task LoadLightSwitchesAsync(InstalledAppInstance installedApp,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            MyState state,
            dynamic data,
            bool shouldSubscribeToEvents = true)
        {
            Logger.LogInformation("Loading lightSwitches...");
            state.LightSwitches = new List<LightSwitch>();

            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));
            _ = state ??
                throw new ArgumentNullException(nameof(state));
            _ = data ??
                throw new ArgumentNullException(nameof(data));

            var lightSwitches = data.installedApp.config.switches;
            if (lightSwitches != null)
            {
                var index = 0;

                foreach (var device in lightSwitches)
                {
                    dynamic deviceConfig = device.deviceConfig;
                    var deviceId = deviceConfig.deviceId.Value;
                    deviceConfig.capability = "switch";
                    deviceConfig.attribute = "switch";
                    deviceConfig.stateChangeOnly = true;
                    deviceConfig.value = "*";
                    deviceConfig.subscriptionName = $"MySwitches{index}";
                    index++;

                    var deviceTasks = new Task<dynamic>[] {
                        SmartThingsAPIHelper.GetDeviceDetailsAsync(
                            installedApp,
                            deviceId),
                        SmartThingsAPIHelper.GetDeviceStatusAsync(
                            installedApp,
                            deviceId)
                    };

                    Task.WaitAll(deviceTasks);

                    var ls = LightSwitch.SwitchFromDynamic(
                        deviceTasks[0].Result,
                        deviceTasks[1].Result);

                    state.LightSwitches.Add(ls);

                    if (shouldSubscribeToEvents)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(() => SubscribeToDeviceEventAsync(installedApp, deviceConfig).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }

            Logger.LogInformation($"Loaded {state.LightSwitches.Count} lightSwitches...");
        }

        public async Task HandleInstallUpdateDataAsync(InstalledAppInstance installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true)
        {
            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));

            var state = await stateManager.GetStateAsync(installedApp.InstalledAppId).ConfigureAwait(false);

            if (state == null)
            {
                state = new MyState()
                {
                    InstalledAppId = installedApp.InstalledAppId
                };
            }

            state.IsAppEnabled = bool.Parse(data.installedApp.config.isAppEnabled[0].stringConfig.value.Value);

            var loadTasks = new Task[] {
                LoadLightSwitchesAsync(installedApp,
                    state,
                    data,
                    shouldSubscribeToEvents)
            };

            Task.WaitAll(loadTasks);

            Logger.LogDebug($"MyState: {state.ToJson()}");

            await stateManager.StoreStateAsync(installedApp.InstalledAppId, state).ConfigureAwait(false);

            Logger.LogInformation($"Updated config for installedApp: {installedApp.InstalledAppId}...");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task HandleUpdateDataAsync(InstalledAppInstance installedApp,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            dynamic data,
            bool shouldSubscribeToEvents = true)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = data ?? throw new ArgumentNullException(nameof(data));

            Logger.LogInformation($"Updating installedApp: {installedApp.InstalledAppId}...");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => HandleInstallUpdateDataAsync(installedApp, data, shouldSubscribeToEvents).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public override async Task HandleInstallDataAsync(InstalledAppInstance installedApp,
            dynamic data,
            bool shouldSubscribeToEvents = true)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = data ?? throw new ArgumentNullException(nameof(data));

            Logger.LogInformation($"Installing installedApp: {installedApp.InstalledAppId}...");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => HandleInstallUpdateDataAsync(installedApp, data, shouldSubscribeToEvents).ConfigureAwait(false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
