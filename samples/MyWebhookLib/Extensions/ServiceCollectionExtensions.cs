#region Copyright
// <copyright file="ServiceCollectionExtensions.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of MyWebhookLib
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
            IConfiguration config,
            bool isFunctionsApp = false)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));

            services
                .Configure<CryptoUtilsConfig>(config.GetSection(nameof(CryptoUtilsConfig)))
                .Configure<SmartAppConfig>(config.GetSection(nameof(SmartAppConfig)))
                .Configure<FileBackedConfig<FileBackedInstalledAppManager>>(config.GetSection("FileBackedInstalledAppManager.FileBackedConfig"))
                .Configure<FileBackedConfig<FileBackedStateManager<MyState>>>(config.GetSection("FileBackedStateManager.FileBackedConfig"))
                .AddSingleton<IConfigWebhookHandler, MyConfigWebhookHandler>()
                .AddSingleton<IInstallUpdateWebhookHandler, MyInstallUpdateDataHandler>()
                .AddSingleton<IUninstallWebhookHandler, MyUninstallWebhookHandler>()
                .AddSingleton<IEventWebhookHandler, MyEventWebhookHandler>()
                .AddFileBackedInstalledAppManager()
                .AddFileBackedStateManager<MyState>()
                .AddSingleton<IMyService, MyService>()
                .AddWebhookHandlers();

            if(isFunctionsApp)
            {                
                services.AddInstalledAppTokenManager();
            }
            else
            {
                services.AddInstalledAppTokenManagerService();
            }

            return services;
        }
    }
}
