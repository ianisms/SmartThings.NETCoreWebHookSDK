using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebhookHandlers(this IServiceCollection services)
        {
            services
                .AddSingleton<IPingWebhookHandler, PingWebhookHandler>()
                .AddSingleton<IOAuthWebhookHandler, OAuthWebhookHandler>()
                .AddSingleton<ICryptoUtils, CryptoUtils>()
                .AddSingleton<ISmartThingsAPIHelper, SmartThingsAPIHelper>()
                .AddHostedService<InstalledAppTokenManager>()
                .AddSingleton<IRootWebhookHandler, RootWebhookHandler>()
                .AddHttpClient();
            return services;
        }

        public static IServiceCollection AddInMemoryInstalledAppManager(this IServiceCollection services)
        {
            services
                .AddSingleton<IInstalledAppManager, InMemoryInstalledAppManager>();
            return services;
        }

        public static IServiceCollection AddFileBackedInstalledAppManager(this IServiceCollection services)
        {
            services
                .AddSingleton<IInstalledAppManager, FileBackedInstalledAppManager>();
            return services;
        }

        public static IServiceCollection AddAzureStorageBackedInstalledAppManager(this IServiceCollection services)
        {
            services
                .AddSingleton<IInstalledAppManager, AzureStorageBackedInstalledAppManager>();
            return services;
        }

        public static IServiceCollection AddInMemoryStateManager<T>(this IServiceCollection services)
        {
            services
                .AddSingleton<IStateManager<T>, InMemoryStateManager<T>>();
            return services;
        }

        public static IServiceCollection AddFileBackedStateManager<T>(this IServiceCollection services)
        {
            services
                .AddSingleton<IStateManager<T>, FileBackedStateManager<T>>();
            return services;
        }

        public static IServiceCollection AddAzureStorageStateManager<T>(this IServiceCollection services)
        {
            services
                .AddSingleton<IStateManager<T>, AzureStorageBackedStateManager<T>>();
            return services;
        }
    }
}
