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
