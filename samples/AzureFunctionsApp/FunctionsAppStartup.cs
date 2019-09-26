using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Security;

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
                .AddSingleton<ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers.IConfigWebhookHandler,
                    AzureFunctionsApp.WebhookHandlers.ConfigWebhookHandler>()
                .AddWebhookHandlers();
        }
    }
}
