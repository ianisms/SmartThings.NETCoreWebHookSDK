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
