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
        ILogger<ISmartThingsAPIHelper> logger { get; }
        SmartAppConfig appConfig { get; }
        IHttpClientFactory httpClientFactory { get; }
        Task<InstalledApp> RefreshTokensAsync(InstalledApp installedApp);
        Task<HttpResponseMessage> SubscribeToDeviceEventAsync(InstalledApp installedApp,
            dynamic device);
        Task<HttpResponseMessage> ClearSubscriptionsAsync(InstalledApp installedApp);
        Task<dynamic> GetDeviceDetailsAsync(InstalledApp installedApp,
            string deviceId);
        Task<dynamic> GetDeviceStatusAsync(InstalledApp installedApp,
            string deviceId);
        Task DeviceCommandAsync(InstalledApp installedApp,
            string deviceId,
            dynamic command);
        Task<Location> GetLocationAsync(InstalledApp installedApp,
            string locationId);
    }

    public class SmartThingsAPIHelper : ISmartThingsAPIHelper
    {
        public ILogger<ISmartThingsAPIHelper> logger { get; private set; }
        public SmartAppConfig appConfig { get; private set; }
        public IHttpClientFactory httpClientFactory { get; private set; }

        public SmartThingsAPIHelper(ILogger<ISmartThingsAPIHelper> logger,
            IOptions<SmartAppConfig> options,
            IHttpClientFactory httpClientFactory)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _ = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            this.logger = logger;
            this.appConfig = options.Value;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<InstalledApp> RefreshTokensAsync(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));

            logger.LogDebug($"Refreshing tokens for installedApp: {installedApp.InstalledAppId}...");

            var uri = new Uri($"https://auth-global.api.smartthings.com/oauth/token");

            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                request.SetBasicAuthHeader(appConfig.SmartAppClientId,
                    appConfig.SmartAppClientSecret);
                var data = $"grant_type=refresh_token&client_id={appConfig.SmartAppClientId}&client_secret={appConfig.SmartAppClientSecret}&refresh_token={installedApp.RefreshToken.TokenValue}";
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var response = await httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        logger.LogError($"Error trying to refresh tokens...  Response details: {errorBody}");
                    }
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic tokenDetails = JObject.Parse(body);
                    _ = tokenDetails.access_token ?? throw new InvalidOperationException("tokenDetails.access_token == null!");
                    _ = tokenDetails.refresh_token ?? throw new InvalidOperationException("tokenDetails.refresh_token == null!");
                    _ = tokenDetails.expires_in ?? throw new InvalidOperationException("tokenDetails.expires_in == null!");

                    logger.LogDebug($"Setting tokens for installedApp: {installedApp.InstalledAppId}...");

                    installedApp.SetTokens(tokenDetails.access_token.Value,
                        tokenDetails.refresh_token.Value,
                        tokenDetails.expires_in.Value);
                    return installedApp;
                }
            }
        }

        public async virtual Task<HttpResponseMessage> SubscribeToDeviceEventAsync(InstalledApp installedApp,
            dynamic device)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = device ?? throw new ArgumentNullException(nameof(device));

            logger.LogDebug($"Subscribing to device: {device.Id}...");

            var uri = new Uri($"https://api.smartthings.com/installedapps/{installedApp.InstalledAppId}/subscriptions");

            dynamic payload = new JObject();
            payload.sourceType = "DEVICE";
            payload.device = device;
            using (var jsonContent = ((JObject)payload).ToStringContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Authorization = 
                        new AuthenticationHeaderValue("Bearer", installedApp.AccessToken.TokenValue);
                    request.Content = jsonContent;
                    using (var httpClient = httpClientFactory.CreateClient())
                    {
                        var response = await httpClient.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorBody = await response.Content.ReadAsStringAsync();
                            dynamic responseDetails = JObject.Parse(errorBody);
                            logger.LogError($"Error trying to subscribe to device...  Response details: {errorBody}");
                        }
                        response.EnsureSuccessStatusCode();
                        return response;
                    }
                }
            }
        }

        public async virtual Task<HttpResponseMessage> ClearSubscriptionsAsync(InstalledApp installedApp)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));

            logger.LogDebug($"Clearing device subscriptions for installedApp: {installedApp.InstalledAppId}...");

            var uri = new Uri($"https://api.smartthings.com/installedapps/{installedApp.InstalledAppId}/subscriptions");

            using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var response = await httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        dynamic responseDetails = JObject.Parse(errorBody);
                        logger.LogError($"Error trying to clear subscriptions...  Response details: {errorBody}");
                    }
                    response.EnsureSuccessStatusCode();
                    return response;
                }
            }
        }

        public async virtual Task<dynamic> GetDeviceDetailsAsync(InstalledApp installedApp,
            string deviceId)
        {
            _ = installedApp ?? 
                throw new ArgumentNullException(nameof(installedApp));
            _ = installedApp.AccessToken ?? 
                throw new ArgumentException("installedApp.AccessToken is null", nameof(installedApp));
            _ = deviceId ?? 
                throw new ArgumentNullException(nameof(deviceId));

            logger.LogDebug($"Getting device details for device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}");

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var response = await httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        logger.LogError($"Error trying to get device details...  Response details: {errorBody}");
                    }
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic device = JObject.Parse(body);
                    return device;
                }
            }
        }

        public async virtual Task<dynamic> GetDeviceStatusAsync(InstalledApp installedApp,
            string deviceId)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = deviceId ?? throw new ArgumentNullException(nameof(deviceId));

            logger.LogDebug($"Getting device status for device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}/status");

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                request.Headers.Authorization = 
                    new AuthenticationHeaderValue("Bearer", installedApp.AccessToken.TokenValue);
                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var response = await httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        dynamic responseDetails = JObject.Parse(errorBody);
                        logger.LogError($"Error trying to get device status...  Response details: {errorBody}");
                    }
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic device = JObject.Parse(body);
                    return device;
                }
            }
        }

        public async virtual Task DeviceCommandAsync(InstalledApp installedApp,
            string deviceId,
            dynamic command)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            _ = command ?? throw new ArgumentNullException(nameof(command));

            logger.LogDebug($"Sending command: {command} to device: {deviceId}...");

            var uri = new Uri($"https://api.smartthings.com/devices/{deviceId}/commands");

            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

                request.Content = ((JObject)command).ToStringContent();

                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var response = await httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        dynamic responseDetails = JObject.Parse(errorBody);
                        logger.LogError($"Error trying to exec command on device...  Response details: {errorBody}");
                    }
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async virtual Task<Location> GetLocationAsync(InstalledApp installedApp,
            string locationId)
        {
            _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
            _ = locationId ?? throw new ArgumentNullException(nameof(locationId));

            logger.LogDebug($"Getting location: {locationId}...");

            var uri = new Uri($"https://api.smartthings.com/locations/{locationId}");

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                request.SetBearerAuthHeader(installedApp.AccessToken.TokenValue);

                using (var httpClient = httpClientFactory.CreateClient())
                {
                    var response = await httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        dynamic responseDetails = JObject.Parse(errorBody);
                        logger.LogError($"Error trying to get location...  Response details: {errorBody}");
                    }
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Location>(body,
                        Common.JsonSerializerSettings);
                }
            }
        }
    }
}
