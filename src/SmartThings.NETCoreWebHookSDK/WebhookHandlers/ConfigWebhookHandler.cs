#region Copyright
// <copyright file="ConfigWebhookHandler.cs" company="Ian N. Bennett">
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
using Microsoft.Extensions.Logging;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IConfigWebhookHandler
    {
        ILogger<IConfigWebhookHandler> Logger { get; }
        dynamic HandleRequest(dynamic request);
        dynamic Initialize(dynamic request);
        dynamic Page(dynamic request);
    }

    public abstract class ConfigWebhookHandler : IConfigWebhookHandler
    {
        public ILogger<IConfigWebhookHandler> Logger { get; private set; }

        protected ConfigWebhookHandler(ILogger<IConfigWebhookHandler> logger)
        {
            this.Logger = logger;
        }

        public virtual void ValidateRequest(dynamic request)
        {
            Logger.LogTrace($"Validating request: {request}");

            _ = request ??
                throw new ArgumentNullException(nameof(request));
            _ = request.configurationData ??
                throw new InvalidOperationException("Missing configurationData!");
            _ = request.configurationData.phase ??
                throw new InvalidOperationException("Missing configurationData.phase!");
        }

        public dynamic HandleRequest(dynamic request)
        {
            ValidateRequest(request);

            Logger.LogDebug("Handling config request...");
            Logger.LogTrace($"Handling request: {request}");

            var phase = request.configurationData.phase.Value.ToLowerInvariant();

            Logger.LogDebug($"Config phase: {phase}");

            dynamic response;

            if (phase == "initialize")
            {
                response = Initialize(request);
            }
            else
            {
                _ = request.configurationData.pageId ??
                    throw new InvalidOperationException("request.configurationData.pageId is null!");
                _ = request.configurationData.pageId.Value ??
                    throw new InvalidOperationException("request.configurationData.pageId is null!");

                response = Page(request);
            }

            Logger.LogTrace($"Response: {response}");

            return response;
        }

        public abstract dynamic Initialize(dynamic request);

        public abstract dynamic Page(dynamic request);
    }
}
