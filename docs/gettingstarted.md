## SmartThings SmartApps

This SDK is used to build a WebHook SmartApp for SmartThings using  NET Core.  For details on how to register your SmartApp and how it runs within SmartThings, please read the [SmartApp documentation](https://smartthings.developer.samsung.com/docs/smartapps/smartapp-basics.html)

## Samples

You can find samples for ASP.NET Core and Azure functions in the samples directory:

- [.NET Core 2.2 - master branch](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/master/samples)
- [.NET Core 3.0 - 3.0 branch](https://github.com/ianisms/SmartThings.NETCoreWebHookSDK/tree/3.0/samples)

Please note: support for .NET Core 3.0 in Azure Functions is currently [in preview](https://dev.to/azure/develop-azure-functions-using-net-core-3-0-gcm).

## Setup Steps

1. Add an instance of ```CryptoUtilsConfig``` via ```Services.Configure``` like so: ```.Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))```.  The details on the properties of ```CryptoUtilsConfig``` can be found [below](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=cryptoutilsconfig).
2. Add an instance of ```SmartAppConfig``` via ```.Configure<SmartAppConfig>(config.GetSection(nameof(SmartAppConfig)))```.    The details on the properties of ```SmartAppConfig``` can be found [below](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=smartappconfig).
3. Add an instance of your implementation of ```ConfigWebhookHandler```, ```InstallUpdateWebhookHandler```, ```UninstallWebhookHandler``` and ```EventWebhookHandler``` via ```Services.Configure``` like in the example below.  Details on the implementation classes can be found as follows:
   - [```ConfigWebhookHandler```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=configwebhookhandler-implementation)
   - [```InstallUpdateWebhookHandler```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=installupdatewebhookhandler-implementation)
   - [```UninstallWebhookHandler```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=uninstallwebhookhandler-implementation)
   - [```EventWebhookHandler```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=eventwebhookhandler-implementation)
4. Add an [```InstalledAppManager```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=installed-app-management-utils)
5. Add a [```InstalledAppTokenManager```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=installed-app-token-management-utils) for either ASP.NET Core (hosted servrce) or Azure Functions (used in a timer trigger you create) to refresh your tokens periodically.
6. Optionally add a [```StateManager```](https://ianisms.github.io/SmartThings.NETCoreWebHookSDK/#/details?id=state-management-utils)
7. Add the remaining handlers via the ```ianisms.SmartThings.NETCoreWebHookSDK.Extensions.AddWebhookHandlers``` extension method using ```.AddWebhookHandlers()```.
8. Pass the ```HttpRequest``` from your ASP.NET Core or FunctionsApp to the ```RootWebhookHandler```.

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

## SmartApp Registration

The details on how to test your app with SmartThings can be found in the [SmartApp documentation](https://smartthings.developer.samsung.com/docs/testing/how-to-test.html).

### ngrok

To test your app with SmartThings while running the app in your environment, I reccomend using [ngrok](https://ngrok.com/product) to tunnel http / https requests to your app.

To use ngrok, run the command like so:

```batch
ngrok http 5000 --host-header=localhost
```

This tells ngrok to tunnel requests via http to port 5000 on your local machine use localhost as the host header.  If your app is not running on localhost, yo can ommit the ```--host-header=localhost``` argument.

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
