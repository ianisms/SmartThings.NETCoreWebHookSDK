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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class AzureStorageBackedTests
    {
        private readonly Mock<ILogger<IInstalledAppManager>> mockIALogger;
        private readonly Mock<ILogger<IStateManager<string>>> mockStateLogger;
        private readonly Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>>> mockIAOptions;
        private readonly Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<string>>>> mockStateOptions;
        private readonly Mock<ISmartThingsAPIHelper> mockSmartThingsAPIHelper;
        private readonly IInstalledAppManager installedAppManager;
        private readonly IStateManager<string> stateManager;

        public AzureStorageBackedTests(ITestOutputHelper output)
        {
            mockIALogger = new Mock<ILogger<IInstalledAppManager>>();
            mockStateLogger = new Mock<ILogger<IStateManager<string>>>();
            mockIALogger.Setup(log => log.Log(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()))
                .Callback<Microsoft.Extensions.Logging.LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, e, state, ex, f) =>
                {
                    output.WriteLine($"{logLevel} logged: \"{state}\"");
                });
            mockStateLogger.Setup(log => log.Log(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()))
                .Callback<Microsoft.Extensions.Logging.LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, e, state, ex, f) =>
                {
                    output.WriteLine($"{logLevel} logged: \"{state}\"");
                });

            mockSmartThingsAPIHelper = new Mock<ISmartThingsAPIHelper>();
            mockSmartThingsAPIHelper.Setup(api => api.RefreshTokensAsync(It.IsAny<InstalledAppInstance>()))
                .Returns(() =>
                {
                    return Task.FromResult<InstalledAppInstance>(GetValidInstalledAppInstance());
                });

            var iaConfig = new AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>()
            {
                CacheBlobName = "installedapps",
                ConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "smartthingssdktest"
            };

            var stateConfig = new AzureStorageBackedConfig<AzureStorageBackedStateManager<string>>()
            {
                CacheBlobName = "state",
                ConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "smartthingssdktest"
            };

            mockIAOptions = new Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>>>();
            mockIAOptions.Setup(opt => opt.Value)
                .Returns(iaConfig);

            mockStateOptions = new Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<string>>>>();
            mockStateOptions.Setup(opt => opt.Value)
                .Returns(stateConfig);

            installedAppManager = new AzureStorageBackedInstalledAppManager(mockIALogger.Object,
                mockSmartThingsAPIHelper.Object,
                mockIAOptions.Object);

            stateManager = new AzureStorageBackedStateManager<string>(mockStateLogger.Object,
                mockStateOptions.Object);

            var installedApp = GetValidInstalledAppInstance();
            installedAppManager.StoreInstalledAppAsync(installedApp).Wait();
            stateManager.StoreStateAsync(installedApp.InstalledAppId, GetStateObject()).Wait();
        }

        private static InstalledAppInstance installedAppInstance;
        public static InstalledAppInstance GetValidInstalledAppInstance()
        {
            if (installedAppInstance == null)
            {
                installedAppInstance = new InstalledAppInstance()
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
            }

            installedAppInstance.SetTokens(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), (long)TimeSpan.FromDays(99).TotalMilliseconds);

            return installedAppInstance;
        }

        private static string stateObject;
        public static string GetStateObject()
        {
            if (stateObject == null)
            {
                stateObject = "stateObject";
            }

            return stateObject;
        }

        public static IEnumerable<object[]> ValidInstalledAppInstance()
        {
            var installedApp = GetValidInstalledAppInstance();

            return new List<object[]>
            {
                new object[] { installedApp }
            };
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IAGetInstalledAppAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var result = await installedAppManager.GetInstalledAppAsync(installedApp.InstalledAppId);
            Assert.NotNull(result);
            Assert.Equal(installedApp, result);
        }

        [Fact]
        public async Task IAPersistCacheAsync_ShouldNotError()
        {
            await installedAppManager.PersistCacheAsync();
        }

        [Fact]
        public async Task IARefreshAllInstalledAppTokensAsync_ShouldNotError()
        {
            await installedAppManager.RefreshAllInstalledAppTokensAsync();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IARefreshTokensAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            await installedAppManager.RefreshTokensAsync(installedApp);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IAStoreInstalledAppAsyncc_ShouldNotError(InstalledAppInstance installedApp)
        {
            await installedAppManager.StoreInstalledAppAsync(installedApp);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IARemoveInstalledAppAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            await installedAppManager.LoadCacheAsync();
            await installedAppManager.RemoveInstalledAppAsync(installedApp.InstalledAppId);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task StateGetStateAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var result = await stateManager.GetStateAsync(installedApp.InstalledAppId);
            Assert.NotNull(result);
            Assert.Equal(GetStateObject(), result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task StateStoreStateAsync_ShouldNotErrorAndShouldNotify(InstalledAppInstance installedApp)
        {
            var observer = new StateObserver(mockStateLogger.Object);
            stateManager.Subscribe(observer);
            await stateManager.StoreStateAsync(installedApp.InstalledAppId, GetStateObject());
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task StateRemoveStateAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            await stateManager.RemoveStateAsync(installedApp.InstalledAppId);
        }
    }

    public class StateObserver : IObserver<string>
    {
        private readonly ILogger logger;
        public StateObserver(ILogger logger)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));

            this.logger = logger;
        }

        public void OnCompleted()
        {
            logger.LogDebug("StateObserver.OnCompleted");
        }

        public void OnError(Exception error)
        {
            logger.LogDebug("StateObserver.OnError");
        }

        public void OnNext(string value)
        {
            logger.LogDebug("StateObserver.OnNext");
            Assert.Equal(AzureStorageBackedTests.GetValidInstalledAppInstance().InstalledAppId, value);
        }
    }
}
