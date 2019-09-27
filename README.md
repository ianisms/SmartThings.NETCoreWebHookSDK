# SmartThings.NETCoreWebHookSDK

#### UPDATES

- Scrapped the object model and now using ```dynamic JObject``` instead.  [More info on  why](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK#fyi-why-am-i-using-dynamic-jobject).
- Got ahead of myself, decided NuGet was a bit premature.  Nuget Packages deprecated until I am ready to release a beta.

***

## Description

Currently just a first pass with 2 samples, one for ASP NET Core Web API and one for an Azure Functions HttpTrigger.  Both samples allow for a very basic WebHook based SmartApp that uses a single-page configuration to display a single section with a boolean toggle.

I will expand on the README and tune the functionality once I port my favorite groovy based SmartApp using this SDK.  Of course I am also happy to take on contributors.  Please use the [issues feature in the repo](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/issues) to report any issues.

I'll also create a release pipeline to push to NuGet once I have something stable.

## Getting Started

### SmartThings SmartApps

This SDK is used to build a WebHook SmartApp for SmartThings using  NET Core.  For details on how to register your SmartApp and how it runs within SmartThings, please read the [SmartApp documentation](https://smartthings.developer.samsung.com/docs/smartapps/smartapp-basics.html)

### DI

This SDK utilizes ```Microsoft.Extensions.DependencyInjection``` for DI as it makes for seamless integration for ASP NET Core and Azure Functions .NET Core apps.  If you favor a different DI library, I welcome you to create a port.

To add the SDK WebHook functionality to your app there are 3 steps all involving DI:

1. Add an instance of ```CryptoUtilsConfig``` via ```Services.Configure``` like so: ```.Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))```.  The details on the properties of ```CryptoUtilsConfig``` can be found [below](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#cryptoutilsconfig).
2. Add an instance of your implementation of ```ConfigWebhookHandler```, ```InstallWebhookHandler```, ```UpdateWebhookHandler```, ```UninstallWebhookHandler``` and ```EventWebhookHandler``` via ```Services.Configure``` like in the example below.  Details on the implementation classes can be foundas follows:
   - [```ConfigWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#configwebhookhandler-implementation)
   - [```InstallWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#installwebhookhandler-implementation)
   - [```UpdateWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#updatewebhookhandler-implementation)
   - [```UninstallWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#uninstallwebhookhandler-implementation)
   - [```EventWebhookHandler```](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/blob/master/README.md#eventwebhookhandler-implementation)
3. Add the remaining handlers via the ```ianisms.SmartThings.NETCoreWebHookSDK.Extensions.AddWebhookHandlers``` extension method like so ```.AddWebhookHandlers()```.

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
            .AddSingleton<IConfigWebhookHandler, MyConfigWebhookHandler>()
            .AddSingleton<IInstallWebhookHandler, MyInstallWebhookHandler>()
            .AddSingleton<IUpdateWebhookHandler, MyUpdateWebhookHandler>()
            .AddSingleton<IUninstallWebhookHandler, MyUninstallWebhookHandler>()
            .AddSingleton<IEventWebhookHandler, MyEventWebhookHandler>()
            .AddWebhookHandlers();
    }
}
```

### CryptoUtilsConfig

```CryptoUtilsConfig``` is used to configure the ```ICryptoUtils``` implementation used to verify the signature on the incoming requests as per the [HTTP signature verification spec](https://smartthings.developer.samsung.com/docs/smartapps/webhook-apps.html#HTTP-signature-verification).

The properties it expects are as follows:

| Property | Description |
|----------------------------------------|---------------------------------------------------------------|
| PublicKeyFilePath | The path to the public key file conatining the public key copied from the SmartApp registration. |

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

### InstallWebhookHandler Implementation

During the [INSTALL Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#INSTALL), the ```InstallWebhookHandler``` you add via DI will react to the handle the ```installData``` sent in the request.  This ```intsallData``` contains the ```installedApp.config``` you should use to create device subscriptions, etc.  Your ```InstallWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.InstallWebhookHandler``` and override the abstract methods ```void HandleInstallData(dynamic installData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

An example:

```csharp
public class MyInstallWebhookHandler : InstallWebhookHandler
{
    public MyInstallWebhookHandler(ILogger<InstallWebhookHandler> logger) : base(logger)
    {
    }

    public override void ValidateRequest(dynamic request)
    {
        base.ValidateRequest((JObject)request);

        _ = request.installData.installedApp.config.isAppEnabled ??
            throw new InvalidOperationException("request.installData.installedApp.config.isAppEnabled is null");
        _ = request.installData.installedApp.config.presenceSensors ??
            throw new InvalidOperationException("request.installData.installedApp.config.presenceSensors is null");
    }

    public override void HandleInstallData(dynamic installData)
    {
        // TODO: Subscribe to device events, etc.
    }
}
```

### UpdateWebhookHandler Implementation

During the [UPDATE Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#UPDATE), the ```UpdateWebhookHandler``` you add via DI will react to the handle the ```updateData``` sent in the request.  This ```updateData``` contains the ```installedApp.config``` you should use to create device subscriptions, etc.  Your ```UpdateWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.UpdateWebhookHandler``` and override the abstract methods ```void HandleUpdateData(dynamic updateData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

An example:

```csharp
public class MyUpdateWebhookHandler : UpdateWebhookHandler
{
    public MyUpdateWebhookHandler(ILogger<UpdateWebhookHandler> logger) : base(logger)
    {
    }

    public override void ValidateRequest(dynamic request)
    {
        base.ValidateRequest((JObject)request);

        _ = request.updateData.installedApp.config.isAppEnabled ??
            throw new InvalidOperationException("request.updateData.installedApp.config.isAppEnabled is null");
        _ = request.updateData.installedApp.config.presenceSensors ??
            throw new InvalidOperationException("request.updateData.installedApp.config.presenceSensors is null");
    }

    public override void HandleUpdateData(dynamic updateData)
    {
        // TODO: Subscribe to device events, etc.
    }
}
```

### UninstallWebhookHandler Implementation

During the [UNINSTALL Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#UNINSTALL), the ```UninstallWebhookHandler``` you add via DI will react to the handle the ```uninstallData``` sent in the request.  This ```intsallData``` contains the ```installedApp.config``` you should use to create device subscriptions, etc.  Your ```UninstallWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.UninstallWebhookHandler``` and override the abstract methods ```void HandleUninstallData(dynamic uninstallData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

An example:

```csharp
public class MyUninstallWebhookHandler : UninstallWebhookHandler
{
    public MyUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger) : base(logger)
    {
    }

    public override void ValidateRequest(dynamic request)
    {
        base.ValidateRequest((JObject)request);

        _ = request.uninstallData.installedApp.config.isAppEnabled ??
            throw new InvalidOperationException("request.uninstallData.installedApp.config.isAppEnabled is null");
        _ = request.installData.installedApp.config.presenceSensors ??
            throw new InvalidOperationException("request.uninstallData.installedApp.config.presenceSensors is null");
    }

    public override void HandleUninstallData(dynamic uninstallData)
    {
        // TODO: Subscribe to device events, etc.
    }
}
```

### EventWebhookHandler Implementation

During the [EVENT Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html#EVENT), the ```EventWebhookHandler``` you add via DI will react to the handle the ```eventData``` sent in the request.  This ```eventData``` contains the ```installedApp.config``` and ```events``` you should use to respond to events.  Your ```EventWebhookHandler``` implementation should extend the abstract class ```ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.EventWebhookHandler``` and override the abstract methods ```void HandleEventData(dynamic eventData)```.  You can also override ```void ValidateRequest(dynamic request)``` to perform additional config validation or the like.

An example:

```csharp
public class MyEventWebhookHandler : EventWebhookHandler
{
    public MyEventWebhookHandler(ILogger<EventWebhookHandler> logger) : base(logger)
    {
    }

    public override void ValidateRequest(dynamic request)
    {
        base.ValidateRequest((JObject)request);

        _ = request.eventData.installedApp.config.isAppEnabled ??
            throw new InvalidOperationException("request.eventData.installedApp.config.isAppEnabled is null");
        _ = request.eventData.installedApp.config.presenceSensors ??
            throw new InvalidOperationException("request.eventData.installedApp.config.presenceSensors is null");
    }

    public override void HandleEventData(dynamic installData)
    {
        // TODO: Subscribe to device events, etc.
    }
}
```

#### FYI: Why am I Using ```dynamic JObject```

I initially published this with a strongly typed object model that was mapped from the [API documentation](https://smartthings.developer.samsung.com/docs/smartapps/lifecycles.html).  I found it to be quite convoluted so, I adopted  ```dynamic JObject``` to insualte developers from these complexities.  Happy to hear strong opinions either way.
