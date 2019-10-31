
# InstallUpdateWebhookHandler Implementation

During the [INSTALL/UPDATE Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#INSTALL), the ```InstallUpdateWebhookHandler``` you add via DI will react to the handle the ```installData``` or ```updateData``` sent in the request.  This ```intsallData``` or ```updateData``` contains the ```installedApp.config``` or ```updatedApp.config``` you should use to create device subscriptions, etc.  Your ```InstallUpdateWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.InstallUpdateWebhookHandler``` and override the abstract methods ```void HandleInstallData(dynamic installData)``` and ```void HandleUpdateData(dynamic updateData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.  You can then parse the ```installData``` or ```updateData``` into [device models](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK#device-models) for use in your app.

## Example

```csharp
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

private async Task SubscribeToDeviceEventAsync(InstalledApp installedApp,
    dynamic deviceConfig)
{
    _ = installedApp ??
        throw new ArgumentNullException(nameof(installedApp));
    _ = deviceConfig ??
        throw new ArgumentNullException(nameof(deviceConfig));

    var resp = await smartThingsAPIHelper.SubscribeToDeviceEventAsync(
        installedApp,
        deviceConfig);

    var body = await resp.Content.ReadAsStringAsync();
    dynamic subscriptionResp = JObject.Parse(body);

    _ = subscriptionResp.id ??
        throw new InvalidOperationException("subscriptionResp.id is null!");
}

private async Task LoadLightSwitchesAsync(InstalledApp installedApp,
    MyState state,
    dynamic data,
    bool shouldSubscribeToEvents = true)
{
    logger.LogInformation("Loading lightSwitches...");
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
                smartThingsAPIHelper.GetDeviceDetailsAsync(
                    installedApp,
                    deviceId),
                smartThingsAPIHelper.GetDeviceStatusAsync(
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
                Task.Run(() => SubscribeToDeviceEventAsync(installedApp, deviceConfig));
            }
        }
    }

    logger.LogInformation($"Loaded {state.LightSwitches.Count} lightSwitches...");
}

public async Task HandleInstallUpdateDataAsync(InstalledApp installedApp,
    dynamic data,
    bool shouldSubscribeToEvents = true)
{
    _ = installedApp ??
        throw new ArgumentNullException(nameof(installedApp));

    var state = await stateManager.GetStateAsync(installedApp.InstalledAppId);

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

    logger.LogDebug($"MyState: {state.ToJson()}");

    await stateManager.StoreStateAsync(installedApp.InstalledAppId, state);

    logger.LogInformation($"Updated config for installedApp: {installedApp.InstalledAppId}...");
}

public override async Task HandleUpdateDataAsync(InstalledApp installedApp,
    dynamic data,
    bool shouldSubscribeToEvents = true)
{
    _ = installedApp ??
        throw new ArgumentNullException(nameof(installedApp));
    _ = data ??
        throw new ArgumentNullException(nameof(data));

    logger.LogInformation($"Updating installedApp: {installedApp.InstalledAppId}...");

    Task.Run(() => HandleInstallUpdateDataAsync(installedApp, data, shouldSubscribeToEvents));
}

public override async Task HandleInstallDataAsync(InstalledApp installedApp,
    dynamic data,
    bool shouldSubscribeToEvents = true)
{
    _ = installedApp ??
        throw new ArgumentNullException(nameof(installedApp));
    _ = data ??
        throw new ArgumentNullException(nameof(data));

    logger.LogInformation($"Installing installedApp: {installedApp.InstalledAppId}...");

    Task.Run(() => HandleInstallUpdateDataAsync(installedApp, data, shouldSubscribeToEvents));
}
```
