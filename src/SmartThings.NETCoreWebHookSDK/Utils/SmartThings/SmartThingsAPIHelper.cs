#region Copyright
// <copyright file="SmartThingsAPIHelper.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings
{
    public interface ISmartThingsAPIHelper
    {
        Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp);
        Task<HttpResponseMessage> SubscribeToDeviceEventAsync(InstalledAppInstance installedApp,
            dynamic device);
        Task<HttpResponseMessage> ClearSubscriptionsAsync(InstalledAppInstance installedApp);
        Task<dynamic> GetDeviceDetailsAsync(InstalledAppInstance installedApp,
            string deviceId);
        Task<dynamic> GetDeviceStatusAsync(InstalledAppInstance installedApp,
            string deviceId);
        Task DeviceCommandAsync(InstalledAppInstance installedApp,
            string deviceId,
            dynamic command);
        Task<Location> GetLocationAsync(InstalledAppInstance installedApp,
            string locationId);
        Task SendNotificationAsync(InstalledAppInstance installedApp,
            string msg,
            string title);
    }

    public class SmartThingsAPIHelper : ISmartThingsAPIHelper
    {
        private readonly ILogger<ISmartThingsAPIHelper> _logger;
        private readonly SmartAppConfig _appConfig;
        private readonly HttpClient _httpClient;

        public SmartThingsAPIHelper(ILogger<ISmartThingsAPIHelper> logger,
            IOptions<SmartAppConfig> options,
            HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfig = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));

            if (installedApp.AccessToken.IsExpired ||
                installedApp.RefreshToken.IsExpired)
            {
                _logger.LogDebug($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

                var uri = new Uri($"https://auth-global.api.smartthings.com/oauth/token");

                using var request = new HttpRequestMessage(HttpMethod.Post, uri);

                request.SetBasicAuthHeader(_appConfig.SmartAppClientId,
                    _appConfig.SmartAppClientSecret);

                var data = $"grant_type=refresh_token&client_id={_appConfig.SmartAppClientId}&client_secret={_appConfig.SmartAppClientSecret}&refresh_token={installedApp.RefreshToken.TokenValue}";
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger.LogError($"Error trying to refresh tokens...  Response body: {errorBody}");
                }

                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic tokenDetails = JObject.Parse(body);

                _ = tokenDetails.access_token ?? throw new InvalidOperationException("tokenDetails.access_token == null!");
                _ = tokenDetails.refresh_token ?? throw new InvalidOperationException("tokenDetails.refresh_token == null!");
                _ = tokenDetails.expires_in ?? throw new InvalidOperationException("tokenDetails.expires_in == null!");

                _logger.LogDebug($"Setting tokens for installedApp: {installedApp.InstalledAppId}...");

                installedApp.SetTokens(tokenDetails.access_token.Value,
                    tokenDetails.refresh_token.Value,
                    tokenDetails.expires_in.Value);
            }
            else
            {

                _logger.LogDebug($"NOT Refreshing tokens for installedApp: {installedApp.InstalledAppId}, tokens not expired...");
            }

            return installedApp;
        }

        public async virtual Task<HttpResponseMessage> SubscribeToDeviceEventAsync(InstalledAppInstance installedApp,
            dynamic device)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = device ?? throw new ArgumentNullException(nameof(device));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Subscribing to device: {device.Id}...");

            var uri = new Uri($"https://api.smartthings.com/installedapps/{installedApp.InstalledAppId}/subscriptions");

            dynamic payload = new JObject();
            payload.sourceType = "DEVICE";
            payload.device = device;

            using var jsonContent = ((JObject)payload).ToStringContent();

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", installedApp.AccessToken.TokenValue);

            request.Content = jsonContent;

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError($"Error trying to subscribe to device...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();

            return response;
        }

        public async virtual Task<HttpResponseMessage> ClearSubscriptionsAsync(InstalledAppInstance installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Clearing device subscriptions for installedApp: {installedApp.InstalledAppId}...");

            var uri = new Uri($"https://api.smartthings.com/installedapps/{installedApp.InstalledAppId}/subscriptions");

            using var request = new HttpRequestMessage(HttpMethod.Delete, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic responseDetails = JObject.Parse(errorBody);
                _logger.LogError($"Error trying to clear subscriptions...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();

            return response;
        }

        public async virtual Task<dynamic> GetDeviceDetailsAsync(InstalledAppInstance installedApp,
            string deviceId)
        {
            _ = installedApp ??
                throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.AccessToken ??
                throw new ArgumentException("installedApp.AccessToken is null",
                nameof(installedApp));
            _ = deviceId ??
                throw new ArgumentNullException(nameof(deviceId));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Getting device details for device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError($"Error trying to get device details...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            dynamic device = JObject.Parse(body);
            return device;
        }

        public async virtual Task<dynamic> GetDeviceStatusAsync(InstalledAppInstance installedApp,
            string deviceId)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = deviceId ?? throw new ArgumentNullException(nameof(deviceId));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Getting device status for device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}/status");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", installedApp.AccessToken.TokenValue);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError($"Error trying to get device status...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            dynamic device = JObject.Parse(body);
            return device;
        }

        public async virtual Task DeviceCommandAsync(InstalledAppInstance installedApp,
            string deviceId,
            dynamic command)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            _ = command ?? throw new ArgumentNullException(nameof(command));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Sending command: {command} to device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}/commands");

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            request.Content = ((JObject)command).ToStringContent();

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError($"Error trying to exec command on device...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();
        }

        public async virtual Task<Location> GetLocationAsync(InstalledAppInstance installedApp,
            string locationId)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = locationId ?? throw new ArgumentNullException(nameof(locationId));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Getting location: {locationId}...");

            var uri = new Uri($"https://api.smartthings.com/locations/{locationId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError($"Error trying to get location...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if(string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidOperationException("ST locations api returned an empty response");
            }

            return Location.LocationFromDynamic(JObject.Parse(body));
        }

        public async virtual Task SendNotificationAsync(InstalledAppInstance installedApp,
            string msg,
            string title = "Automation Message")
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = msg ?? throw new ArgumentNullException(nameof(msg));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            _logger.LogDebug($"Sending notification: {msg}...");

            var uri = new Uri($"https://api.smartthings.com/notification");

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue); 
            
            var json = $@"{{
                ""type"": ""AUTOMATION_INFO"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""title"": ""{title}"",
			    ""message"": ""{msg}""
            }}";

            dynamic notification = JObject.Parse(json);

            request.Content = ((JObject)notification).ToStringContent();

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError($"Error trying to send notification...  Response body: {errorBody}");
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
