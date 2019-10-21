# ianisms.SmartThings.NETCoreWebHookSDK

#### UPDATES

- 21 October 2019:  Massive updates after working on a SmartApp using the SDK.  README and samples completely updated.
- 21 October 2019:  Looking at creating a version for .NET Core 3.0.  Support for 3.0 in Azure Functions is in alpha.  Will create a PoC soon.
- 21 October 2019:  [NuGet package](https://www.nuget.org/packages/SmartThings.NETCoreWebHookSDK/2.2.0-alpha) created:
  
  ```
  dotnet add package SmartThings.NETCoreWebHookSDK --version 2.2.0-alpha
  ```

***

## Description

Currently just a first pass with 2 samples, one for ASP NET Core Web API and one for an Azure Functions HttpTrigger.  Both samples allow for a very basic WebHook based SmartApp that uses a single-page configuration to display a single section with a boolean toggle and a list of switches.  The samples also show how to subscribe to and handle events for the switches.

I will expand on the README and tune the functionality once I port my favorite groovy based SmartApp using this SDK.  Of course I am also happy to take on contributors.  Please use the [issues feature in the repo](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/issues) to report any issues.

I'll also create a release pipeline to push to NuGet once I have something stable.

## Getting Started

### SmartThings SmartApps

This SDK is used to build a WebHook SmartApp for SmartThings using  NET Core.  For details on how to register your SmartApp and how it runs within SmartThings, please read the [SmartApp documentation](https://smartthings.developer.samsung.com/docs/smartapps/smartapp-basics.html)

### DI

This SDK utilizes ```Microsoft.Extensions.DependencyInjection``` for DI as it makes for seamless integration for ASP NET Core and Azure Functions .NET Core apps.  If you favor a different DI library, I welcome you to create a port.

To add the SDK WebHook functionality to your app there are 3 steps all involving DI:

1. Add an instance of ```CryptoUtilsConfig``` via ```Services.Configure``` like so: ```.Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))```.  The details on the properties of ```CryptoUtilsConfig``` can be found [below](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#cryptoutilsconfig).
2. Add an instance of ```SmartAppConfig``` via ```.Configure<SmartAppConfig>(config.GetSection(nameof(SmartAppConfig)))```.    The details on the properties of ```SmartAppConfig``` can be found [below](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#smartappconfig).
3. Add an instance of your implementation of ```ConfigWebhookHandler```, ```InstallUpdateWebhookHandler```, ```UninstallWebhookHandler``` and ```EventWebhookHandler``` via ```Services.Configure``` like in the example below.  Details on the implementation classes can be found as follows:
   - [```ConfigWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#configwebhookhandler-implementation)
   - [```InstallUpdateWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#installupdatewebhookhandler-implementation)
   - [```UninstallWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#uninstallwebhookhandler-implementation)
   - [```EventWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#eventwebhookhandler-implementation)
4. Add an [```InstalledAppManager```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#installed-app-management-tils)
5. Optionally add a [```StateManager```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#state-management-utils)
6. Add the remaining handlers via the ```ianisms.SmartThings.NETCoreWebHookSDK.Extensions.AddWebhookHandlers``` extension method like so ```.AddWebhookHandlers()```.

A full example:

```csharp
public class FunctionsAppStartup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        var config = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        builder.Services
            .AddLogging()
            .Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))
            .Configure<SmartAppConfig>(config.GetSection(nameof(SmartAppConfig)))
            .AddSingleton<IConfigWebhookHandler, MyConfigWebhookHandler>()
            .AddSingleton<IInstallUpdateWebhookHandler, MyInstallUpdateDataHandler>()
            .AddSingleton<IUninstallWebhookHandler, MyUninstallWebhookHandler>()
            .AddSingleton<IEventWebhookHandler, MyEventWebhookHandler>()
            .AddInMemoryInstalledAppManager()
            .AddSingleton<IStateManager<MyState>, InMemoryStateManager<MyState>>()
            .AddSingleton<IMyService, MyService>()
            .AddWebhookHandlers();
    }
}
```

## Details

### CryptoUtilsConfig

```CryptoUtilsConfig``` is used to configure the ```ICryptoUtils``` implementation used to verify the signature on the incoming requests as per the [HTTP signature verification spec](https://smartthings.developer.samsung.com/docs/smartapps/webhook-apps.html#HTTP-signature-verification).

The properties it expects are as follows:

| Property | Description |
|----------------------------------------|---------------------------------------------------------------|
| PublicKeyFilePath | The path to the public key file conatining the public key copied from the SmartApp registration. |

### SmartAppConfig

The ```SmartAppConfig``` should be configured with The ```SmartAppClientId``` and ```SmartAppClientSecret``` given to you in the webhook registration on the developer portal.  This is used to, among other things, refresh the tokens for your app.  For example:

```json
"SmartAppConfig": {
    "SmartAppClientId": "<YOURCLIENTID>",
    "SmartAppClientSecret": "<YOURCLIENTSECRET>"
},
```

### ConfigWebhookHandler Implementation

During the [CONFIGURATION Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/configuration.html), the ```ConfigWebhookHandler``` you add via DI will send desired config screens to the UI to allow for app configuration.  Your ```ConfigWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler``` and override the two abstract methods ```dynamic Initialize(dynamic request)``` and ```dynamic Page(dynamic request)```.

For ```Initialize``` you should return a ```Newtonsoft.Json.Linq dynamic JObject``` like so:

```csharp
dynamic response = JObject.Parse(@"{
    'configurationData': {
        'initialize': {
            'id': 'app',
            'name': 'The Great Welcomer',
            'permissions': ['r:devices:*'],
            'firstPageId': '1'
        }
    }
}");
```

For ```Page``` you should return a ```Newtonsoft.Json.Linq dynamic JObject``` respresenting your desired conguration screens like so for a single-page configuration:

```csharp
dynamic response = JObject.Parse(@"{
    'configurationData': {
        'page': {
            'pageId': '1',
            'name': 'Configure The Great Welcomer',
            'nextPageId': null,
            'previousPageId': null,
            'complete': true,
            'sections' : [
                {
                    'name': 'basics',
                    'settings' : [
                        {
                            'id': 'appEnabled',
                            'name': 'Enabled App?',
                            'description': 'Easy toggle to enable/disable the app',
                            'type': 'BOOLEAN',
                            'required': true,
                            'defaultValue': true,
                            'multiple': false
                        },
                        {
                            'id': 'switches',
                            'name': 'Light Switches',
                            'description': 'The switches for the app to turn on/off',
                            'type': 'DEVICE',
                            'required': true,
                            'multiple': true,
                            'capabilities': ['switch'],
                            'permissions': ['r', 'x']
                        }
                    ]
                }
            ]
        }
    }
}");
```

If you are using a multi-page confguration, you should look at the ```pageId``` property in thhe ```dynamic request``` parameter and return the request page configuration being sure to set ```'nextPageId'``` and ```'previousPageId'``` accordingly.  Set ```'complete'``` to ```true``` on the final page and ```false``` on all other pages.

Multi-page example:

```csharp
private dynamic PageOne()
{
    dynamic response = JObject.Parse(@"{
        'configurationData': {
            'page': {
                'pageId': '1',
                'name': 'Basics',
                'nextPageId': '2',
                'previousPageId': null,
                'complete': false,
                'sections' : [
                    {
                        'name': 'Basics',
                        'settings' : [
                            {
                                'id': 'isAppEnabled',
                                'name': 'Enabled App?',
                                'description': 'Easy toggle to enable/disable the app',
                                'type': 'BOOLEAN',
                                'required': true,
                                'defaultValue': true,
                                'multiple': false
                            },
                            {
                                'id': 'locks',
                                'name': 'Which Lock(s)?',
                                'description': 'The lock(s) that will be unlocked after arrival.',
                                'type': 'DEVICE',
                                'required': false,
                                'multiple': true,
                                'capabilities': ['lock'],
                                'permissions': ['r', 'x']
                            },
                        ]
                    }
                ]
            }
        }
    }");

    return response;
}

private dynamic PageTwo()
{
    dynamic response = JObject.Parse(@"{
        'configurationData': {
            'page': {
                'pageId': '2',
                'name': 'Motion and Switches',
                'nextPageId': '3',
                'previousPageId': '1',
                'complete': false,
                'sections' : [
                    {
                        'name': 'Motion',
                        'settings' : [
                            {
                                'id': 'motionSensors',
                                'name': 'Which Motion Sensor(s)?',
                                'description': 'The motion sensor(s) that will trigger the app on presence + motion.  If empty, the presence sesnor alone will trigger the app.',
                                'type': 'DEVICE',
                                'required': false,
                                'multiple': true,
                                'capabilities': ['motionSensor'],
                                'permissions': ['r']
                            }
                        ]
                    },
                    {
                        'name': 'Switches',
                        'settings' : [
                            {
                                'id': 'switches',
                                'name': 'Which Light Switch(es)?',
                                'description': 'The switch(es) to turn on/off on arrival.',
                                'type': 'DEVICE',
                                'required': false,
                                'multiple': true,
                                'capabilities': ['switch'],
                                'permissions': ['r', 'x']
                            }
                        ],
                    }
                ]
            }
        }
    }");

    return response;
}

public override dynamic Page(dynamic request)
{
    var pageId = request.configurationData.pageId.Value;

    switch (pageId)
    {
        case "1":
            return PageOne();
        case "2":
            return PageTwo();
        default:
            throw new InvalidOperationException($"Unknown pageId: {request.configurationData.pageId.Value}");
    }
}
```

More details on the ```CONFIGURATION``` phase can be found [here](https://smartthings.developer.samsung.com/docs/smartapps/configuration.html#Configuration).

Full Example:

```csharp
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AzureFunctionsApp.WebhookHandlers
{
    public class ConfigWebhookHandler :
        ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler
    {
        public ConfigWebhookHandler(ILogger<ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler> logger) : base(logger)
        {
        }

        public override dynamic Initialize(dynamic request)
        {
            dynamic response = JObject.Parse(@"{
                'configurationData': {
                    'initialize': {
                        'id': 'app',
                        'name': 'The Great Welcomer',
                        'permissions': ['r:devices:*'],
                        'firstPageId': '1'
                    }
                }
            }");

            return response;
        }

        public override dynamic Page(dynamic request)
        {
            dynamic response = JObject.Parse(@"{
                'configurationData': {
                    'page': {
                        'pageId': '1',
                        'name': 'Configure The Great Welcomer',
                        'nextPageId': null,
                        'previousPageId': null,
                        'complete': true,
                        'sections' : [
                            {
                                'name': 'basics',
                                'settings' : [
                                    {
                                        'id': 'appEnabled',
                                        'name': 'Enabled App?',
                                        'description': 'Easy toggle to enable/disable the app',
                                        'type': 'BOOLEAN',
                                        'required': true,
                                        'defaultValue': true,
                                        'multiple': false
                                    },
                                    {
                                        'id': 'switches',
                                        'name': 'Light Switches',
                                        'description': 'The switches for the app to turn on/off',
                                        'type': 'DEVICE',
                                        'required': true,
                                        'multiple': true,
                                        'capabilities': ['switch'],
                                        'permissions': ['r', 'x']
                                    }
                                ]
                            }
                        ]
                    }
                }
            }");

            return response;
        }
    }
}
```

### InstallUpdateWebhookHandler Implementation

During the [INSTALL Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#INSTALL), the ```InstallUpdateWebhookHandler``` you add via DI will react to the handle the ```installData``` or ```updateData``` sent in the request.  This ```intsallData``` or ```updateData``` contains the ```installedApp.config``` or ```updatedApp.config``` you should use to create device subscriptions, etc.  Your ```InstallUpdateWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.InstallUpdateWebhookHandler``` and override the abstract methods ```void HandleInstallData(dynamic installData)``` and ```void HandleUpdateData(dynamic updateData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.  You can then parse the ```installData``` or ```updateData``` into [device models](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#devicemodels) for use in your app.

An example:

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
    _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
    _ = data ?? throw new ArgumentNullException(nameof(data));

    logger.LogInformation($"Updating installedApp: {installedApp.InstalledAppId}...");

    Task.Run(() => HandleInstallUpdateDataAsync(installedApp, data, shouldSubscribeToEvents));
}

public override async Task HandleInstallDataAsync(InstalledApp installedApp,
    dynamic data,
    bool shouldSubscribeToEvents = true)
{
    _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
    _ = data ?? throw new ArgumentNullException(nameof(data));

    logger.LogInformation($"Installing installedApp: {installedApp.InstalledAppId}...");

    Task.Run(() => HandleInstallUpdateDataAsync(installedApp, data, shouldSubscribeToEvents));
}
}
```

### UninstallWebhookHandler Implementation

During the [UNINSTALL Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#UNINSTALL), the ```UninstallWebhookHandler``` you add via DI will react to the handle the ```uninstallData``` sent in the request.  This ```uninstallData``` contains the ```installedApp.config``` you should use to remove state, etc.  Your ```UninstallWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.UninstallWebhookHandler``` and override the abstract methods ```void HandleUninstallData(dynamic uninstallData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

An example:

```csharp
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
```

### EventWebhookHandler Implementation

During the [EVENT Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#EVENT), the ```EventWebhookHandler``` you add via DI will react to the handle the ```eventData``` sent in the request.  This ```eventData``` contains the ```installedApp.config``` and ```events``` you should use to respond to events.  Your ```EventWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.EventWebhookHandler``` and override the abstract methods ```void HandleEventData(dynamic eventData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

An example:

```csharp
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
```

### Installed App Management Utils

I have included an insatlled app manager utility for helping you store/retrive app configs for your app.  There are 3 variations available:  ```InMemoryInstalledAppManager```, ```FileBackedInstalledAppManager```, and ```AzureStorageBackedInstalledAppManager```.  This isnatlled app manager is used throughout the SDK and at least one must be configured.

#### InMemoryInstalledAppManager Usage

To use the InMemoryInstalledAppManager, you simply it like so: ```.AddInMemoryInstalledAppManager()```.

#### FileBackedInstalledAppManager Usage

To use the FileBackedInstalledAppManager, you must add a ```FileBackedConfig``` to your service collection like so: ```.Configure<FileBackedConfig<FileBackedInstalledAppManager>>(config.GetSection("FileBackedInstalledAppManager.FileBackedConfig"))```.  You can then inject the ```InstalledAppManager``` like so: ```.AddFileBackedInstalledAppManager()```.

#### AzureStorageBackedInstalledAppManager Usage

To use the AzureStorageBackedInstalledAppManager, you must add a ```AzureStorageBackedConfig``` to your service collection like so: ```.Configure<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>>(config.GetSection("AzureStorageBackedInstalledAppManager.AzureStorageConfig"))```.  You can then inject the ```InstalledAppManager``` like so: ```.AddAzureStorageInstalledAppManager()```.

### State Management Utils

I have included a state manager utility for helping you store/retrive state for your app.  There are 3 variations available:  ```InMemoryStateManager```, ```FileBackedStateManager```, and ```AzureStorageBackedStateManager```.

#### InMemoryStateManager Usage

To use the InMemoryStateManager, you simply it like so: ```.AddInMemoryStateManager()```.

#### FileBackedStateManager Usage

To use the FileBackedStateManager, you must add a ```FileBackedConfig``` to your service collection like so: ```.Configure<FileBackedConfig<FileBackedStateManager<MyState>>>(config.GetSection("FileBackedStateManager.FileBackedConfig"))```. ```MyState``` would be your custom state object.  You can then inject the ```StateManager``` like so: ```.AddFileBackedStateManager<MyState>()```.

#### AzureStorageBackedStateManager Usage

To use the AzureStorageBackedStateManager, you must add a ```AzureStorageBackedConfig``` to your service collection like so: ```.Configure<AzureStorageBackedConfig<AzureStorageBackedStateManager<MyState>>>(config.GetSection("AzureStorageBackedStateManager.AzureStorageConfig"))```. ```MyState``` would be your custom state object.  You can then inject the ```StateManager``` like so: ```.AddAzureStorageStateManager<MyState>()```.

### Device Models

The SDK has stringly typed models for several devices which can be instantiated from the responses coming from the SmartThings API.  These devices also have commands that can be executed on them for example locking a lock and switching on a light switch.  The models can be found in ```ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings```, the listis as follows with more to be added soon:

  - ContactSensor
  - DoorLock
  - LightSwitch
  - MotionSensor
  - PresenceSensor
  - SpeechDevice
