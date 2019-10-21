using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyWebhookLib.Extensions;
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
                .AddMyWebhookService(config);
        }
    }
}
