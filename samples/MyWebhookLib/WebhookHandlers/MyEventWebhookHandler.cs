#region Copyright
// <copyright file="MyEventWebhookHandler.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ianisms.SmartThings.GreatWelcomer.Lib.WebhookHandlers
{
    public class MyEventWebhookHandler : EventWebhookHandler
    {
        private IStateManager<MyState> stateManager;
        private IInstallUpdateWebhookHandler installUpdateHandler;

        public MyEventWebhookHandler(ILogger<EventWebhookHandler> logger,
            IInstalledAppManager installedAppManager,
            IStateManager<MyState> stateManager,
            IInstallUpdateWebhookHandler installUpdateHandler)
            : base(logger, installedAppManager)
        {
            this.stateManager = stateManager;
            this.installUpdateHandler = installUpdateHandler;
        }

        public override void ValidateRequest(dynamic request)
        {
            base.ValidateRequest((JObject)request);

            _ = request.eventData.installedApp.config.isAppEnabled ??
                throw new InvalidOperationException($"request.eventData.installedApp.config.isAppEnabled is null");
            _ = request.eventData.installedApp.config.switches ??
                throw new InvalidOperationException($"request.eventData.installedApp.config.switches is null");
        }

        public override async Task HandleEventDataAsync(InstalledApp installedApp,
            dynamic eventData)
        {
            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));
            _ = eventData ??
                throw new ArgumentNullException(nameof(eventData));

            logger.LogDebug($"Handling eventData for installedApp: {installedApp.InstalledAppId}...");

            var state = await stateManager.GetStateAsync(installedApp.InstalledAppId);

            if (state == null)
            {
                await installUpdateHandler.HandleUpdateDataAsync(installedApp, eventData, false);
                state = await stateManager.GetStateAsync(installedApp.InstalledAppId);
            }

            _ = state ??
                throw new InvalidOperationException($"Unable to retrieve state for app: {installedApp.InstalledAppId}");

            var raisedEvents = eventData.events;

            logger.LogDebug($"Handling raisedEvents for installedApp: {installedApp.InstalledAppId}...");

            var raisedEvent = raisedEvents[0];
            if (raisedEvent.deviceEvent != null)
            {
                logger.LogDebug($"Handling raisedEvent for installedApp: {installedApp.InstalledAppId}:  {raisedEvent.deviceEvent}");
                await HandleDeviceEventAsync(state, raisedEvent.deviceEvent);
            }
        }

        private async Task HandleDeviceEventAsync(MyState state, dynamic deviceEvent)
        {
            _ = state ??
                throw new ArgumentNullException(nameof(state));
            _ = deviceEvent ??
                throw new ArgumentNullException(nameof(deviceEvent));
            _ = deviceEvent.subscriptionName ??
                throw new ArgumentException($"deviceEvent.subscriptionName is null!", nameof(deviceEvent));

            var subscriptionName = deviceEvent.subscriptionName.Value;

            if (subscriptionName.StartsWith("MySwitches", StringComparison.Ordinal))
            {
                if (state.LightSwitches == null)
                {
                    logger.LogDebug("No light switches configured, ignoring event...");
                }
                else
                {
                    logger.LogDebug($"Checking light switch: {deviceEvent}...");

                    var lightSwitch =
                        state.LightSwitches.SingleOrDefault(ls =>
                            ls.Id == deviceEvent.deviceId.Value);

                    _ = lightSwitch ??
                        throw new InvalidOperationException($"Could not find configured lightSwitch with id: {deviceEvent.deviceId.Value}");

                    lightSwitch.CurrentState =
                        LightSwitch.SwitchStateFromDynamic(deviceEvent.value);

                    await stateManager.StoreStateAsync(state.InstalledAppId, state);
                }
            }
            else
            {
                throw new InvalidOperationException($"Unexpected subscriptionName: {subscriptionName}!");
            }
        }
    }
}
