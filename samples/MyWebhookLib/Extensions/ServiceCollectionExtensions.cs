using ianisms.SmartThings.GreatWelcomer.Lib.WebhookHandlers;
using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using MyWebhookLib.WebhookHandlers;
using MyWebhookLib.Models;
using MyWebhookLib.Services;

namespace MyWebhookLib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyWebhookService(this IServiceCollection services,
            IConfiguration config)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));

            services
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
            return services;
        }
    }
}
