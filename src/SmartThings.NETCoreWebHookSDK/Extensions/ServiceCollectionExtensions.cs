#region Copyright
// <copyright file="ServiceCollectionExtensions.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// SmartThings.NETCoreWebHookSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SmartThings.NETCoreWebHookSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
#endregion

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
