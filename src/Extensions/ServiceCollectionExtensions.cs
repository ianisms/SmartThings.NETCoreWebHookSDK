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
                .AddSingleton<PingWebhookHandler>()
                .AddSingleton<InstallWebhookHandler>()
                .AddSingleton<UpdateWebhookHandler>()
                .AddSingleton<EventWebhookHandler>()
                .AddSingleton<OAuthWebhookHandler>()
                .AddSingleton<UninstallWebhookHandler>();
            return services;
        }
    }
}
