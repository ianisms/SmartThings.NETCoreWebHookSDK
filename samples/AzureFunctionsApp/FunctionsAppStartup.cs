using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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

            builder.Services
                .AddLogging()
                .AddWebhookHandlers()
                .AddSingleton<AzureFunctionsApp.WebhookHandlers.ConfigWebhookHandler>();
        }
    }
}
