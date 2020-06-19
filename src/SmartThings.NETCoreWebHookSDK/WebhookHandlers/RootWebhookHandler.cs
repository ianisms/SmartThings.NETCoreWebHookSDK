#region Copyright
// <copyright file="RootWebhookHandler.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IRootWebhookHandler
    {
        ILogger<IRootWebhookHandler> Logger { get; }
        Task<dynamic> HandleRequestAsync(HttpRequest request);
    }

    public class RootWebhookHandler : IRootWebhookHandler
    {
        public ILogger<IRootWebhookHandler> Logger { get; private set; }
        private readonly IPingWebhookHandler pingHandler;
        private readonly IConfirmationWebhookHandler confirmationHandler;
        private readonly IConfigWebhookHandler configHandler;
        private readonly IInstallUpdateWebhookHandler installUpdateHandler;
        private readonly IEventWebhookHandler eventHandler;
        private readonly IOAuthWebhookHandler oauthHandler;
        private readonly IUninstallWebhookHandler uninstallHandler;
        private readonly ICryptoUtils cryptoUtils;

        public RootWebhookHandler(ILogger<IRootWebhookHandler> logger,
            IPingWebhookHandler pingHandler,
            IConfirmationWebhookHandler confirmationHandler,
            IConfigWebhookHandler configHandler,
            IInstallUpdateWebhookHandler installUpdateHandler,
            IEventWebhookHandler eventHandler,
            IOAuthWebhookHandler oauthHandler,
            IUninstallWebhookHandler uninstallHandler,
            ICryptoUtils cryptoUtils)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = confirmationHandler ??
                throw new ArgumentNullException(nameof(confirmationHandler));
            _ = pingHandler ??
                throw new ArgumentNullException(nameof(pingHandler));
            _ = configHandler ??
                throw new ArgumentNullException(nameof(configHandler));
            _ = installUpdateHandler ??
                throw new ArgumentNullException(nameof(installUpdateHandler));
            _ = eventHandler ??
                throw new ArgumentNullException(nameof(eventHandler));
            _ = oauthHandler ??
                throw new ArgumentNullException(nameof(oauthHandler));
            _ = uninstallHandler ??
                throw new ArgumentNullException(nameof(uninstallHandler));
            _ = cryptoUtils ??
                throw new ArgumentNullException(nameof(cryptoUtils));

            this.Logger = logger;
            this.confirmationHandler = confirmationHandler;
            this.pingHandler = pingHandler;
            this.configHandler = configHandler;
            this.installUpdateHandler = installUpdateHandler;
            this.eventHandler = eventHandler;
            this.oauthHandler = oauthHandler;
            this.uninstallHandler = uninstallHandler;
            this.cryptoUtils = cryptoUtils;
        }

        public async Task<dynamic> HandleRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            using (var reader = new StreamReader(request.Body))
            {
                var requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                dynamic data = JObject.Parse(requestBody);

                _ = data.lifecycle ??
                    throw new ArgumentException("lifecycle missing from request body!",
                    nameof(request));
                _ = data.lifecycle.Value ??
                    throw new ArgumentException("lifecycle missing from request body!",
                    nameof(request));

                var lifecycleVal = data.lifecycle.Value.ToLowerInvariant().Replace("_", "");

                Lifecycle lifecycle = Lifecycle.Unknown;
                if (!Enum.TryParse<Lifecycle>(lifecycleVal, true, out lifecycle))
                {
                    throw new ArgumentException($"data.lifecycle is an invalid value: {data.lifecycle.Value}",
                        nameof(request));
                }

                switch (lifecycle)
                {
                    case Lifecycle.Ping:
                        return pingHandler.HandleRequest(data);
                    case Lifecycle.Confirmation:
                        return await confirmationHandler.HandleRequestAsync(data);
                    case Lifecycle.Configuration:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return configHandler.HandleRequest(data);
                    case Lifecycle.Install:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await installUpdateHandler.HandleRequestAsync(lifecycle, data);
                    case Lifecycle.Update:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await installUpdateHandler.HandleRequestAsync(lifecycle, data);
                    case Lifecycle.Event:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await eventHandler.HandleRequestAsync(data);
                    case Lifecycle.Uninstall:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await uninstallHandler.HandleRequestAsync(data);
                    case Lifecycle.Oauthcallback:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await oauthHandler.HandleRequestAsync(data);
                    default:
                        break;
                }
            }

            return null;
        }

        private async Task CheckAuthAsync(HttpRequest request)
        {
            if (!await cryptoUtils.VerifySignedRequestAsync(request).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Could not verify request signature!");
            }
        }
    }
}
