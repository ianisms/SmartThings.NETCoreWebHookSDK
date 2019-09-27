using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.DependencyInjection;
using System;

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
                .AddSingleton<IRootWebhookHandler, RootWebhookHandler>();
            return services;
        }
    }
}
