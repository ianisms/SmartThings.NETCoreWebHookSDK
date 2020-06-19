#region Copyright
// <copyright file="ConfirmationWebhookHandler.cs" company="Ian N. Bennett">
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
