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
        private readonly IPingWebhookHandler _pingHandler;
        private readonly IConfirmationWebhookHandler _confirmationHandler;
        private readonly IConfigWebhookHandler _configHandler;
        private readonly IInstallUpdateWebhookHandler _installUpdateHandler;
        private readonly IEventWebhookHandler _eventHandler;
        private readonly IOAuthWebhookHandler _oauthHandler;
        private readonly IUninstallWebhookHandler _uninstallHandler;
        private readonly ICryptoUtils _cryptoUtils;

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
            this.Logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _confirmationHandler = confirmationHandler ??
                throw new ArgumentNullException(nameof(confirmationHandler));
            _pingHandler = pingHandler ??
                throw new ArgumentNullException(nameof(pingHandler));
            _configHandler = configHandler ??
                throw new ArgumentNullException(nameof(configHandler));
            _installUpdateHandler = installUpdateHandler ??
                throw new ArgumentNullException(nameof(installUpdateHandler));
            _eventHandler = eventHandler ??
                throw new ArgumentNullException(nameof(eventHandler));
            _oauthHandler = oauthHandler ??
                throw new ArgumentNullException(nameof(oauthHandler));
            _uninstallHandler = uninstallHandler ??
                throw new ArgumentNullException(nameof(uninstallHandler));
            _cryptoUtils = cryptoUtils ??
                throw new ArgumentNullException(nameof(cryptoUtils));
        }

        public async Task<dynamic> HandleRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            using (var reader = new StreamReader(request.Body))
            {
                var requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                dynamic data = JObject.Parse(requestBody);

                _ = data?.lifecycle?.Value ??
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
                        return _pingHandler.HandleRequest(data);
                    case Lifecycle.Confirmation:
                        return await _confirmationHandler.HandleRequestAsync(data);
                    case Lifecycle.Configuration:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return _configHandler.HandleRequest(data);
                    case Lifecycle.Install:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await _installUpdateHandler.HandleRequestAsync(lifecycle, data);
                    case Lifecycle.Update:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await _installUpdateHandler.HandleRequestAsync(lifecycle, data);
                    case Lifecycle.Event:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await _eventHandler.HandleRequestAsync(data);
                    case Lifecycle.Uninstall:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await _uninstallHandler.HandleRequestAsync(data);
                    case Lifecycle.Oauthcallback:
                        await CheckAuthAsync(request).ConfigureAwait(false);
                        return await _oauthHandler.HandleRequestAsync(data);
                    default:
                        break;
                }
            }

            return null;
        }

        private async Task CheckAuthAsync(HttpRequest request)
        {
            if (!await _cryptoUtils.VerifySignedRequestAsync(request).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Could not verify request signature!");
            }
        }
    }
}
