# Getting Started

## SmartThings SmartApps

This SDK is used to build a WebHook SmartApp for SmartThings using  NET 5.  For details on how to register your SmartApp and how it runs within SmartThings, please read the [SmartApp Basics](https://developer-preview.smartthings.com/docs/connected-services/smartapp-basics), [Hosting a Webhook SmartApp](https://developer-preview.smartthings.com/docs/connected-services/hosting/webhook-smartapp), and [SmartApp Registration](https://developer-preview.smartthings.com/docs/connected-services/app-registration) documentation.

## Samples

You can find samples for ASP.NET Core and Azure functions in the samples directory:

- [.NET 5](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples)

## Setup Steps

1. Add an instance of ```CryptoUtilsConfig``` via ```Services.Configure``` like so: ```.Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))```.  The details on the properties of ```CryptoUtilsConfig``` can be found [below](CryptoUtilsConfig.md).  The SDK uses the SmartThings x.509 cert method of request verification described in the [HTTP signature verification spec](https://developer-preview.smartthings.com/docs/connected-services/hosting/webhook-smartapp#authorizing-calls-from-smartthings).  
2. Add an instance of ```SmartAppConfig``` via ```.Configure<SmartAppConfig>(config.GetSection(nameof(SmartAppConfig)))```.    The details on the properties of ```SmartAppConfig``` can be found [below](SmartAppConfig.md).
3. Add an instance of your implementation of ```ConfigWebhookHandler```, ```InstallUpdateWebhookHandler```, ```UninstallWebhookHandler``` and ```EventWebhookHandler``` via ```Services.Configure``` like in the example below.  Details on the implementation classes can be found as follows:
   - [```ConfigWebhookHandler```](ConfigWebhookHandler.md)
   - [```InstallUpdateWebhookHandler```](InstallUpdateWebhookHandler.md)
   - [```UninstallWebhookHandler```](UninstallWebhookHandler.md)
   - [```EventWebhookHandler```](EventWebhookHandler.md)
4. Add an [```InstalledAppManager```](InstalledAppManagement.md)
5. Add a [```InstalledAppTokenManager```](InstalledAppTokenManagement.md) for either ASP.NET Core (hosted servrce) or Azure Functions (used in a timer trigger you create) to refresh your tokens periodically.
6. Optionally add a [```StateManager```](StateManagement.md)
7. Add the remaining handlers via the ```ianisms.SmartThings.NETCoreWebHookSDK.Extensions.AddWebhookHandlers``` extension method using ```.AddWebhookHandlers()```.
8. Pass the ```HttpRequest``` from your ASP.NET Core or FunctionsApp to the ```RootWebhookHandler```.
9. After deploying or running your SmartApp for the first time, [register your app](https://developer-preview.smartthings.com/docs/connected-services/app-registration) via the SmartThings developer workspace.  The URI will be the endpoint of your ASP.NET Core or FunctionApp.  See [Running Locally](README?id=running-locally) for details on running locally for debugging.

>**NOTE** At the time of this writing, you will need to opt your SmartApp in for SmartHings x.509 cert request verification.  See [Opt in your SmartApp](https://developer-preview.smartthings.com/docs/connected-services/hosting/webhook-smartapp#opt-in-your-smartapp) for more details.

### Example

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
            .AddInstalledAppTokenManager() //.AddInstalledAppTokenManagerService() for ASP.NET Core
            .AddSingleton<IStateManager<MyState>, InMemoryStateManager<MyState>>()
            .AddSingleton<IMyService, MyService>()
            .AddWebhookHandlers();
    }
}
```

Pass ```HttpRequest``` to ```RootWebhookHandler```:

```csharp
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
```

## Running Locally

### ngrok

To test your app with SmartThings while running the app in your environment, I recommend using [ngrok](https://ngrok.com/product) to tunnel http / https requests to your app.

To use ngrok, run the command like so:

```batch
ngrok http 5000 --host-header=localhost
```

This tells ngrok to tunnel requests via http to port 5000 on your local machine use localhost as the host header.  If your app is not running on localhost, you can ommit the ```--host-header=localhost``` argument.

The output will look something like:

```batch
ngrok by @inconshreveable                  (Ctrl+C to quit)

Session Status      online
Account             Ian N Bennett (Plan: Basic)
Version             2.3.35
Region              United States (us)
Web Interface       http://127.0.0.1:4040
Forwarding          http://36f32ad6.ngrok.io -> http://localhost:5000
Forwarding          https://36f32ad6.ngrok.io -> http://localhost:5000
Connections         ttl     opn     rt1     rt5     p50     p90
                    0       0       0.00    0.00    0.00    0.00
```

You can then hit the provided public ngrok endpoint, ```https://36f32ad6.ngrok.io``` in this case, and it will tunnel the requests to your app.  This means that you would set / change your SmartApp registration details to use ```https://36f32ad6.ngrok.io```  in the uri.  In the case of the samples, the uri would be ```https://36f32ad6.ngrok.io/api/FirstWH```.