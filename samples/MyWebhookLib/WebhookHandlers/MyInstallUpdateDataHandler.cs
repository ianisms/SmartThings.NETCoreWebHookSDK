#region Copyright
// <copyright file="MyInstallUpdateDataHandler.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of MyWebhookLib
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
