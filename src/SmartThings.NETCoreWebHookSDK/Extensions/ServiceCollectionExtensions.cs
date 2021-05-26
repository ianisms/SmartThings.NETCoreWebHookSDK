#region Copyright
// <copyright file="ServiceCollectionExtensions.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
#endregion
using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebhookHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IPingWebhookHandler, PingWebhookHandler>();
            services.AddSingleton<IConfirmationWebhookHandler, ConfirmationWebhookHandler>();
            services.AddSingleton<IOAuthWebhookHandler, OAuthWebhookHandler>();
            services.AddSingleton<CryptoUtilsConfigValidator>();
            services.AddHttpClient<ICryptoUtils, CryptoUtils>();
            services.AddSingleton<ICryptoUtils, CryptoUtils>();
            services.AddHttpClient<ISmartThingsAPIHelper, SmartThingsAPIHelper>();
            services.AddSingleton<ISmartThingsAPIHelper, SmartThingsAPIHelper>();
            services.AddSingleton<IRootWebhookHandler, RootWebhookHandler>();
            return services;
        }

        public static IServiceCollection AddInstalledAppTokenManager(this IServiceCollection services)
        {
            services.AddSingleton<IInstalledAppTokenManager, InstalledAppTokenManager>();
            return services;
        }

        public static IServiceCollection AddInstalledAppTokenManagerService(this IServiceCollection services)
        {
            services.AddHostedService<InstalledAppTokenManagerService>();
            return services;
        }

        public static IServiceCollection AddFileBackedInstalledAppManager(this IServiceCollection services)
        {
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IInstalledAppManager, FileBackedInstalledAppManager>();
            return services;
        }

        public static IServiceCollection AddAzureStorageBackedInstalledAppManager(this IServiceCollection services)
        {
            services.AddSingleton<IInstalledAppManager, AzureStorageBackedInstalledAppManager>();
            return services;
        }

        public static IServiceCollection AddFileBackedStateManager<T>(this IServiceCollection services)
        {
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IStateManager<T>, FileBackedStateManager<T>>();
            return services;
        }

        public static IServiceCollection AddAzureStorageStateManager<T>(this IServiceCollection services)
        {
            services.AddSingleton<IStateManager<T>, AzureStorageBackedStateManager<T>>();
            return services;
        }
    }
}
