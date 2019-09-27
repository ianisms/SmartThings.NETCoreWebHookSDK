using AzureFunctionsApp.WebhookHandlers;
using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(AzureFunctionsApp.FunctionsAppStartup))]

namespace AzureFunctionsApp
{
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
}
