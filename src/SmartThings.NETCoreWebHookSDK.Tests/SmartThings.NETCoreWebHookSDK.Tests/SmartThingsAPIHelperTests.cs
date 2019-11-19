#region Copyright
// <copyright file="SmartThingsAPIHelperTests.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class SmartThingsAPIHelperTests
    {
        private readonly string BaseUri = "https://api.smartthings.com";
        private readonly Mock<ILogger<ISmartThingsAPIHelper>> mockLogger;
        private readonly Mock<IOptions<SmartAppConfig>> mockOptions;
        private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
        private readonly IHttpClientFactory mockHttpClientFactory;
        private readonly ISmartThingsAPIHelper smartThingsAPIHelper;

        public SmartThingsAPIHelperTests(ITestOutputHelper output)
        {
            mockLogger = new Mock<ILogger<ISmartThingsAPIHelper>>();
            mockLogger.Setup(log => log.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, e, state, ex, f) =>
                {
                    var formattedLog = state as FormattedLogValues;
                    output.WriteLine($"{logLevel} logged: \"{formattedLog}\"");
                });

            var smartAppConfig = new SmartAppConfig()
            {
                SmartAppClientId = "SACLIENTID",
                SmartAppClientSecret = "SACLIENTSECRET"
            };

            mockOptions = new Mock<IOptions<SmartAppConfig>>();
            mockOptions.Setup(opt => opt.Value)
                .Returns(smartAppConfig);

            mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpClientFactory = mockHttpMessageHandler.CreateClientFactory();
            smartThingsAPIHelper = new SmartThingsAPIHelper(mockLogger.Object,
                mockOptions.Object,
                mockHttpClientFactory);
        }

        public static IEnumerable<object[]> ValidInstalledAppInstance()
        {
            var installedApp = new InstalledAppInstance()
            {
                InstalledAppId = Guid.NewGuid().ToString(),
                InstalledLocation = new Location()
                {
                    CountryCode = "US",
                    Id = Guid.NewGuid().ToString(),
                    Label = "Home",
                    Latitude = 40.347054,
                    Longitude = -74.064308,
                    TempScale = TemperatureScale.F,
                    TimeZoneId = "America/New_York",
                    Locale = "en"
                }
            };

            installedApp.SetTokens(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            return new List<object[]>
            {
                new object[] { installedApp }
            };
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task ClearSubscriptionsAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            var uri = $"{BaseUri}/installedapps/{installedApp.InstalledAppId}/subscriptions";
            mockHttpMessageHandler.SetupRequest(HttpMethod.Delete,
                uri)
                .ReturnsResponse("ok");

            var result = await smartThingsAPIHelper.ClearSubscriptionsAsync(installedApp);

            Assert.NotNull(result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task DeviceCommandAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            var doorLock = new DoorLock()
            {
                CurrentState = LockState.Locked,
                Id = Guid.NewGuid().ToString(),
                Label = "Front door lock"
            };

            mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                $"{BaseUri}/devices/{doorLock.Id}/commands")
                .ReturnsResponse("ok");

            await smartThingsAPIHelper.DeviceCommandAsync(installedApp,
                doorLock.Id,
                DoorLock.GetDeviceCommand(false));
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task GetDeviceDetailsAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var doorLock = new DoorLock()
            {
                CurrentState = LockState.Locked,
                Id = Guid.NewGuid().ToString(),
                Label = "Front door lock"
            };

            var responseBody = $@"{{
                ""deviceId"": ""{doorLock.Id}"",
                ""name"": ""Front.door.lock"",
                ""label"": ""{doorLock.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            dynamic expectedResult = JObject.Parse(responseBody);

            mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
                $"{BaseUri}/devices/{doorLock.Id}")
                .ReturnsResponse(responseBody);

            var result = await smartThingsAPIHelper.GetDeviceDetailsAsync(installedApp,
                doorLock.Id);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task GetDeviceStatusAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var doorLock = new DoorLock()
            {
                CurrentState = LockState.Locked,
                Id = Guid.NewGuid().ToString(),
                Label = "Front door lock"
            };

            var responseBody = $@"{{
                ""deviceId"": ""{doorLock.Id}"",
                ""name"": ""Front.door.lock"",
                ""label"": ""{doorLock.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""lock"": {{
                            ""lock"": {{
                                ""value"": ""unlocked""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic expectedResult = JObject.Parse(responseBody);

            mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
                $"{BaseUri}/devices/{doorLock.Id}/status")
                .ReturnsResponse(responseBody);

            var result = await smartThingsAPIHelper.GetDeviceStatusAsync(installedApp,
                doorLock.Id);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task GetLocationAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
                $"{BaseUri}/locations/{installedApp.InstalledLocation.Id}")
                .ReturnsResponse(JsonConvert.SerializeObject(installedApp.InstalledLocation));

            var result = await smartThingsAPIHelper.GetLocationAsync(installedApp,
                installedApp.InstalledLocation.Id);

            Assert.Equal(installedApp.InstalledLocation, result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task RefreshTokensAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var at = Guid.NewGuid().ToString();
            var rt = Guid.NewGuid().ToString();
            var atExpires = TimeSpan.FromMinutes(5).ToString("c");
            var expiresDt = DateTime.Now.Add(TimeSpan.FromMinutes(5));

            var responseBody = $@"{{
                ""access_token"": ""{at}"",
                ""refresh_token"": ""{rt}"",
                ""expires_in"": ""{atExpires}""
            }}";

            mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                "https://auth-global.api.smartthings.com/oauth/token")
                .ReturnsResponse(responseBody);
            var result = await smartThingsAPIHelper.RefreshTokensAsync(installedApp);

            Assert.NotNull(result);

            var expiresDiff = result.AccessToken.ExpiresDT.Subtract(expiresDt).Duration();

            Assert.Equal(installedApp.AccessToken.TokenValue,
                result.AccessToken.TokenValue);
            Assert.True((expiresDiff <= TimeSpan.FromSeconds(45))); // Due to buffer and proc time
            Assert.False(result.AccessToken.IsExpired);
            Assert.Equal(installedApp.RefreshToken.TokenValue,
                result.RefreshToken.TokenValue);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task SubscribeToDeviceEventAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var doorLock = new DoorLock()
            {
                CurrentState = LockState.Locked,
                Id = Guid.NewGuid().ToString(),
                Label = "Front door lock"
            };

            var deviceConfig = $@"{{
                ""deviceId"": ""{doorLock.Id}"",
                ""name"": ""Front.door.lock"",
                ""label"": ""{doorLock.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""componentId"": ""main"",
                ""capability"": ""lock"",
                ""attribute"": ""lock"",
                ""stateChangeOnly"": ""true"",
                ""value"": ""*"",
                ""subscriptionName"": ""DOORLOCKS_1""
            }}";

            dynamic dynDevice = JObject.Parse(deviceConfig);


            var responseBody = $@"{{
                ""id"": ""{Guid.NewGuid().ToString()}"",
                ""installedAppId"": ""{installedApp.InstalledAppId}"",
                ""sourceType"": ""DEVICE"",
                ""device"": {deviceConfig}
            }}";

            dynamic expectedResult = JObject.Parse(responseBody);

            mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                $"{BaseUri}/installedapps/{installedApp.InstalledAppId}/subscriptions")
                .ReturnsResponse(responseBody);

            var response = await smartThingsAPIHelper.SubscribeToDeviceEventAsync(installedApp,
                dynDevice);


            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic result = JObject.Parse(responseText);

            Assert.Equal(expectedResult, result);
        }
    }
}
