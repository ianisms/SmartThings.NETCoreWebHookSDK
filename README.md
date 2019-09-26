# SmartThings.NETCoreWebHookSDK

### UPDATE

**Nuget Package Published!!!!**: SmartThings.NETCoreWebHookSDK

***

Currently just a first pass with 2 samples, one for ASP NET Core Web API and one for an Azure Functions HttpTrigger.  Both samples allow for a very basic WebHook based SmartApp that uses a single-page configuration to display a single section with a boolean toggle.

I will expand on the README and tune the functionality once I port my favorite groovy based SmartApp using this SDK.  Of course I am also happy to take on contributors.  Please use the [issues feature in the repo](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/issues) to report any issues.

I'll also create a release pipeline to push to NuGet once I have something stable.

## Getting Started

### SmartThings SmartApps

This SDK is used to build a WebHook SmartApp for SmartThings using  NET Core.  For details on how to register your SmartApp and how it runs within SmartThings, please read the [SmartApp documentation](https://smartthings.developer.samsung.com/docs/smartapps/smartapp-basics.html)

### DI

This SDK utilizes ```Microsoft.Extensions.DependencyInjection``` for DI as it makes for seamless integration for ASP NET Core and Azure Functions .NET Core apps.  If you favor a different DI library, I welcome you to create a port.

To add the SDK WebHook functionality to your app there are 3 steps all involving DI:

1. Add an instance of ```CryptoUtilsConfig``` via ```Services.Configure``` like so: ```.Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))```.  The details on the properties of ```CryptoUtilsConfig``` can be found below.
2. Add an instance of your implementation of ```ConfigWebhookHandler``` via ```Services.Configure``` like so: ```.AddSingleton<ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.IConfigWebhookHandler, AzureFunctionsApp.WebhookHandlers.ConfigWebhookHandler>()```.  Details on the implementation of ```ConfigWebhookHandler``` can be found below.
3. Add the rmaining handlers via the ```ianisms.SmartThings.NETCoreWebHookSDK.Extensions.AddWebhookHandlers``` extension method like so ```.AddWebhookHandlers()```.

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

During the [CONFIG Lifecycle phase](https://smartthings.developer.samsung.com/docs/smartapps/configuration.html), the ```ConfigWebhookHandler``` you add via DI will send desired config screens to the UI to allow for app configuration.  The rest of this section is a big TODO for documentation.

Example:

```csharp
using ianisms.SmartThings.NETCoreWebHookSDK.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsApp.WebhookHandlers
{
    public class ConfigWebhookHandler :
        ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler
    {
        public ConfigWebhookHandler(ILogger<ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.ConfigWebhookHandler> logger) : base(logger)
        {
        }

        public override ConfigResponse Initialize()
        {
            var response = new ConfigInitResponse()
            {
                ConfigData = new ConfigInitResponseConfigData()
                {
                    InitData = new ConfigInitResponseData
                    {
                        Name = "My App Name",
                        Id = "app",
                        Permissions = new string[]
                        {
                            "r:devices:*"
                        },
                        FirstPageId = "1"
                    }
                }
            };

            return response;
        }

        public override ConfigResponse Page()
        {
            var response = new ConfigPageResponse()
            {
                ConfigData = new ConfigPageResponseConfigData()
                {
                    Page = new ConfigPage()
                    {
                        PageId = "1",
                        Name = "Configure My App",
                        IsComplete = true,
                        Sections = new ConfigSection[]
                        {
                            new ConfigSection()
                            {
                                Name = "Basics",
                                Settings = new ConfigSetting[]
                                {
                                    new ConfigSetting()
                                    {
                                        Id = "AppEnabled",
                                        Name = "Enable App?",
                                        Type = ConfigSetting.SettingsType.Boolean,
                                        IsRequired = true,
                                        IsMultiple = false,
                                        DefaultValue = "true"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return response;
        }
    }
}
```
