#region Copyright
// <copyright file="RootWebhookHandler.cs" company="Ian N. Bennett">
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
        private readonly IConfigWebhookHandler configHandler;
        private readonly IInstallUpdateWebhookHandler installUpdateHandler;
        private readonly IEventWebhookHandler eventHandler;
        private readonly IOAuthWebhookHandler oauthHandler;
        private readonly IUninstallWebhookHandler uninstallHandler;
        private readonly ICryptoUtils cryptoUtils;

        public RootWebhookHandler(ILogger<IRootWebhookHandler> logger,
            IPingWebhookHandler pingHandler,
            IConfigWebhookHandler configHandler,
            IInstallUpdateWebhookHandler installUpdateHandler,
            IEventWebhookHandler eventHandler,
            IOAuthWebhookHandler oauthHandler,
            IUninstallWebhookHandler uninstallHandler,
            ICryptoUtils cryptoUtils)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
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
