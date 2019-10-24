#region Copyright
// <copyright file="SmartThingsAPIHelper.cs" company="Ian N. Bennett">
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
        ILogger<ISmartThingsAPIHelper> Logger { get; }
        SmartAppConfig AppConfig { get; }
        IHttpClientFactory HttpClientFactory { get; }
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
    }

    public class SmartThingsAPIHelper : ISmartThingsAPIHelper
    {
        public ILogger<ISmartThingsAPIHelper> Logger { get; private set; }
        public SmartAppConfig AppConfig { get; private set; }
        public IHttpClientFactory HttpClientFactory { get; private set; }

        public SmartThingsAPIHelper(ILogger<ISmartThingsAPIHelper> logger,
            IOptions<SmartAppConfig> options,
            IHttpClientFactory httpClientFactory)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _ = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            this.Logger = logger;
            this.AppConfig = options.Value;
            this.HttpClientFactory = httpClientFactory;
        }

        public async Task<InstalledAppInstance> RefreshTokensAsync(InstalledAppInstance installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));

            if (installedApp.AccessToken.IsExpired ||
                installedApp.RefreshToken.IsExpired)
            {
                Logger.LogDebug($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

                var uri = new Uri($"https://auth-global.api.smartthings.com/oauth/token");

                using var request = new HttpRequestMessage(HttpMethod.Post, uri);

                request.SetBasicAuthHeader(AppConfig.SmartAppClientId,
                    AppConfig.SmartAppClientSecret);

                var data = $"grant_type=refresh_token&client_id={AppConfig.SmartAppClientId}&client_secret={AppConfig.SmartAppClientSecret}&refresh_token={installedApp.RefreshToken.TokenValue}";
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                using var httpClient = HttpClientFactory.CreateClient();

                var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.LogError($"Error trying to refresh tokens...  Response body: {errorBody}");
                }
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic tokenDetails = JObject.Parse(body);
                _ = tokenDetails.access_token ?? throw new InvalidOperationException("tokenDetails.access_token == null!");
                _ = tokenDetails.refresh_token ?? throw new InvalidOperationException("tokenDetails.refresh_token == null!");
                _ = tokenDetails.expires_in ?? throw new InvalidOperationException("tokenDetails.expires_in == null!");

                Logger.LogDebug($"Setting tokens for installedApp: {installedApp.InstalledAppId}...");

                installedApp.SetTokens(tokenDetails.access_token.Value,
                    tokenDetails.refresh_token.Value,
                    tokenDetails.expires_in.Value);
            }
            else
            {

                Logger.LogDebug($"NOT Refreshing tokens for installedApp: {installedApp.InstalledAppId}, tokens not expired...");
            }

            return installedApp;
        }

        public async virtual Task<HttpResponseMessage> SubscribeToDeviceEventAsync(InstalledAppInstance installedApp,
            dynamic device)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = device ?? throw new ArgumentNullException(nameof(device));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            Logger.LogDebug($"Subscribing to device: {device.Id}...");

            var uri = new Uri($"https://api.smartthings.com/installedapps/{installedApp.InstalledAppId}/subscriptions");

            dynamic payload = new JObject();
            payload.sourceType = "DEVICE";
            payload.device = device;

            using var jsonContent = ((JObject)payload).ToStringContent();

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", installedApp.AccessToken.TokenValue);

            request.Content = jsonContent;

            using var httpClient = HttpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.LogError($"Error trying to subscribe to device...  Response body: {errorBody}");
            }
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async virtual Task<HttpResponseMessage> ClearSubscriptionsAsync(InstalledAppInstance installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            Logger.LogDebug($"Clearing device subscriptions for installedApp: {installedApp.InstalledAppId}...");

            var uri = new Uri($"https://api.smartthings.com/installedapps/{installedApp.InstalledAppId}/subscriptions");

            using var request = new HttpRequestMessage(HttpMethod.Delete, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            using var httpClient = HttpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic responseDetails = JObject.Parse(errorBody);
                Logger.LogError($"Error trying to clear subscriptions...  Response body: {errorBody}");
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

            Logger.LogDebug($"Getting device details for device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            using var httpClient = HttpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.LogError($"Error trying to get device details...  Response body: {errorBody}");
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

            Logger.LogDebug($"Getting device status for device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}/status");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", installedApp.AccessToken.TokenValue);

            using var httpClient = HttpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.LogError($"Error trying to get device status...  Response body: {errorBody}");
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

            Logger.LogDebug($"Sending command: {command} to device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}/commands");

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            request.Content = ((JObject)command).ToStringContent();

            using var httpClient = HttpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.LogError($"Error trying to exec command on device...  Response body: {errorBody}");
            }
            response.EnsureSuccessStatusCode();
        }

        public async virtual Task<Location> GetLocationAsync(InstalledAppInstance installedApp,
            string locationId)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = locationId ?? throw new ArgumentNullException(nameof(locationId));

            await RefreshTokensAsync(installedApp).ConfigureAwait(false);

            Logger.LogDebug($"Getting location: {locationId}...");

            var uri = new Uri($"https://api.smartthings.com/locations/{locationId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

            using var httpClient = HttpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.LogError($"Error trying to get location...  Response body: {errorBody}");
            }
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<Location>(body,
                STCommon.JsonSerializerSettings);
        }
    }
}
