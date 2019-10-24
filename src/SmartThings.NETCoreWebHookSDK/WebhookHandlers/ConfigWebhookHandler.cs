#region Copyright
// <copyright file="ConfigWebhookHandler.cs" company="Ian N. Bennett">
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

        public ConfigWebhookHandler(ILogger<IConfigWebhookHandler> logger)
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
