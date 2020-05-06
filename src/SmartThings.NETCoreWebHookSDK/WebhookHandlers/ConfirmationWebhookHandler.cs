#region Copyright
// <copyright file="PingWebhookHandler.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers
{
    public interface IConfirmationWebhookHandler
    {
        ILogger<IConfirmationWebhookHandler> logger { get; }
        SmartAppConfig appConfig { get; }
        IHttpClientFactory httpClientFactory { get; }
        Task <dynamic> HandleRequestAsync(dynamic request);
    }

    public class ConfirmationWebhookHandler : IConfirmationWebhookHandler
    {
        public ILogger<IConfirmationWebhookHandler> logger { get; private set; }
        public SmartAppConfig appConfig { get; private set; }
        public IHttpClientFactory httpClientFactory { get; private set; }

        public ConfirmationWebhookHandler(ILogger<IConfirmationWebhookHandler> logger,
            IOptions<SmartAppConfig> options,
            IHttpClientFactory httpClientFactory)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _ = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            this.logger = logger;
            this.appConfig = options.Value;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<dynamic> HandleRequestAsync(dynamic request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            _ = request.confirmationData ?? throw new ArgumentException("request.confirmationData is null", nameof(request));
            _ = request.confirmationData.confirmationUrl ?? throw new ArgumentException("request.confirmationUrl is null", nameof(request));
            
            logger.LogDebug($"handling request: {request}");

            var uri = new Uri((string)request.confirmationData.confirmationUrl);
            dynamic payload = new JObject();
            using var jsonContent = ((JObject)payload).ToStringContent();

            using var confirmRequest = new HttpRequestMessage(HttpMethod.Put, uri);

            confirmRequest.Content = jsonContent;

            confirmRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", appConfig.PAT);

            using var httpClient = httpClientFactory.CreateClient();

            var confirmResponse = await httpClient.SendAsync(confirmRequest).ConfigureAwait(false);
            confirmResponse.EnsureSuccessStatusCode();

            dynamic response = new JObject();
            response.targetUrl = request.confirmationData.confirmationUrl;

            logger.LogDebug($"response: {response}");
            return response;
        }
    }
}
