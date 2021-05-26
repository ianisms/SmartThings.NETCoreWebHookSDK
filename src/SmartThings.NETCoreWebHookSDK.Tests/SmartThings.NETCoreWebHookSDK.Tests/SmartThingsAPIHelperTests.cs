#region Copyright
// <copyright file="SmartThingsAPIHelperTests.cs" company="Ian N. Bennett">
//
// Copyright (C) 2020 Ian N. Bennett
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
        private readonly Mock<ILogger<ISmartThingsAPIHelper>> _mockLogger;
        private readonly Mock<IOptions<SmartAppConfig>> _mockOptions;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _mockHttpClient;
        private readonly ISmartThingsAPIHelper _smartThingsAPIHelper;

        public SmartThingsAPIHelperTests(ITestOutputHelper output)
        {
            _mockLogger = new Mock<ILogger<ISmartThingsAPIHelper>>();
            _mockLogger.Setup(log => log.Log(It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                null,
                It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel,
                    EventId,
                    object,
                    Exception,
                    Func<object, Exception, string>>((logLevel, e, state, ex, f) =>
                    {
                        output.WriteLine($"{logLevel} logged: \"{state}\"");
                    });

            var smartAppConfig = new SmartAppConfig()
            {
                SmartAppClientId = "SACLIENTID",
                SmartAppClientSecret = "SACLIENTSECRET"
            };

            _mockOptions = new Mock<IOptions<SmartAppConfig>>();
            _mockOptions.Setup(opt => opt.Value)
                .Returns(smartAppConfig);

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _mockHttpClient = _mockHttpMessageHandler.CreateClient();
            _smartThingsAPIHelper = new SmartThingsAPIHelper(_mockLogger.Object,
                _mockOptions.Object,
                _mockHttpClient);
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
            _mockHttpMessageHandler.SetupRequest(HttpMethod.Delete,
                uri)
                .ReturnsResponse("ok");

            var result = await _smartThingsAPIHelper.ClearSubscriptionsAsync(installedApp);

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

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                $"{BaseUri}/devices/{doorLock.Id}/commands")
                .ReturnsResponse("ok");

            await _smartThingsAPIHelper.DeviceCommandAsync(installedApp,
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

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
                $"{BaseUri}/devices/{doorLock.Id}")
                .ReturnsResponse(responseBody);

            var result = await _smartThingsAPIHelper.GetDeviceDetailsAsync(installedApp,
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

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
                $"{BaseUri}/devices/{doorLock.Id}/status")
                .ReturnsResponse(responseBody);

            var result = await _smartThingsAPIHelper.GetDeviceStatusAsync(installedApp,
                doorLock.Id);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task GetLocationAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            _mockHttpMessageHandler.SetupRequest(HttpMethod.Get,
                $"{BaseUri}/locations/{installedApp.InstalledLocation.Id}")
                .ReturnsResponse(JsonConvert.SerializeObject(installedApp.InstalledLocation));

            var result = await _smartThingsAPIHelper.GetLocationAsync(installedApp,
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

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                "https://auth-global.api.smartthings.com/oauth/token")
                .ReturnsResponse(responseBody);
            var result = await _smartThingsAPIHelper.RefreshTokensAsync(installedApp);

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
                ""id"": ""{Guid.NewGuid()}"",
                ""installedAppId"": ""{installedApp.InstalledAppId}"",
                ""sourceType"": ""DEVICE"",
                ""device"": {deviceConfig}
            }}";

            dynamic expectedResult = JObject.Parse(responseBody);

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                $"{BaseUri}/installedapps/{installedApp.InstalledAppId}/subscriptions")
                .ReturnsResponse(responseBody);

            var response = await _smartThingsAPIHelper.SubscribeToDeviceEventAsync(installedApp,
                dynDevice);


            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            dynamic result = JObject.Parse(responseText);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void SmartThingsAPIHelper_RefreshTokensAsync_Should_Not_Throw()
        {
            var  installedApp = new InstalledAppInstance()
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

            installedApp.SetTokens(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), -100);

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                "https://auth-global.api.smartthings.com/oauth/token")
                .ReturnsResponse(@"
                {
                    ""access_token"":""foo"",
                    ""refresh_token"":""foo"",
                    ""expires_in"":""1""
                }");

            _smartThingsAPIHelper.RefreshTokensAsync(installedApp);
        }

        [Fact]
        public void SmartThingsAPIHelper_RefreshTokensAsync_Should_HandleError()
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

            installedApp.SetTokens(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), -100);

            _mockHttpMessageHandler.SetupRequest(HttpMethod.Post,
                "https://auth-global.api.smartthings.com/oauth/token")
                .ReturnsResponse(System.Net.HttpStatusCode.InternalServerError,
                    "ERROR");

            _smartThingsAPIHelper.RefreshTokensAsync(installedApp);
        }
    }
}
