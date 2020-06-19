#region Copyright
// <copyright file="UninstallWebhookHandler.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IUninstallWebhookHandler
    {
        ILogger<IUninstallWebhookHandler> Logger { get; }
        IInstalledAppManager InstalledAppManager { get; }
        Task<dynamic> HandleRequestAsync(dynamic request);
        void ValidateRequest(dynamic request);
        Task HandleUninstallDataAsync(dynamic uninstallData);
    }

    public abstract class UninstallWebhookHandler : IUninstallWebhookHandler
    {
        public ILogger<IUninstallWebhookHandler> Logger { get; private set; }
        public IInstalledAppManager InstalledAppManager { get; private set; }

        public UninstallWebhookHandler(ILogger<IUninstallWebhookHandler> logger,
            IInstalledAppManager installedAppManager)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ?? throw new ArgumentNullException(nameof(installedAppManager));

            this.Logger = logger;
            this.InstalledAppManager = installedAppManager;
        }

        public abstract Task HandleUninstallDataAsync(dynamic uninstallData);

        public virtual void ValidateRequest(dynamic request)
        {
            Logger.LogTrace($"Validating request: {request}");

            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.uninstallData ??
                throw new ArgumentException("request.uninstallData is null",
                    nameof(request));
            _ = request.uninstallData.installedApp ??
                throw new ArgumentException("request.uninstallData.installedApp is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.installedAppId ??
                throw new ArgumentException("request.uninstallData.installedApp.installedAppId is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.locationId ??
                throw new ArgumentException("request.uninstallData.installedApp.locationId is null",
                    nameof(request));
            _ = request.uninstallData.installedApp.config ??
                throw new ArgumentException("request.uninstallData.installedApp.config is null",
                    nameof(request));
        }

        public virtual async Task<dynamic> HandleRequestAsync(dynamic request)
        {
            ValidateRequest(request);

            Logger.LogTrace($"Handling request: {request}");

            await HandleUninstallDataAsync(request.uninstallData);

            var installedAppId = (string)request.uninstallData.installedApp.installedAppId;

            await InstalledAppManager.RemoveInstalledAppAsync(installedAppId).ConfigureAwait(false);

            dynamic response = new JObject();

            response.uninstallData = new JObject();

            Logger.LogTrace($"Response: {response}");

            return response;
        }
    }
}
