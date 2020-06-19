#region Copyright
// <copyright file="MyUninstallWebhookHandler.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.Extensions.Logging;
using MyWebhookLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace MyWebhookLib.WebhookHandlers
{
    public class MyUninstallWebhookHandler : UninstallWebhookHandler
    {
        private readonly IStateManager<MyState> stateManager;

        public MyUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger,
            IInstalledAppManager installedAppManager,
            IStateManager<MyState> stateManager)
            : base(logger, installedAppManager)
        {
            _ = stateManager ??
                throw new ArgumentNullException(nameof(stateManager));
            this.stateManager = stateManager;
        }

        public override async Task HandleUninstallDataAsync(dynamic uninstallData)
        {
            var installedAppId = uninstallData.installedApp.installedAppId;
            await stateManager.RemoveStateAsync(installedAppId).ConfigureAwait(false);
        }
    }
}
