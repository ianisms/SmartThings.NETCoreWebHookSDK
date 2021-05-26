#region Copyright
// <copyright file="FileBackedTests.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class FileBackedTests
    {
        private readonly Mock<ILogger<IInstalledAppManager>> _mockIALogger;
        private readonly Mock<ILogger<IStateManager<string>>> _mockStateLogger;
        private readonly Mock<IOptions<FileBackedConfig<FileBackedInstalledAppManager>>> _mockIAOptions;
        private readonly Mock<IOptions<FileBackedConfig<FileBackedStateManager<string>>>> _mockStateOptions;
        private readonly Mock<FileBackedConfigValidator<FileBackedInstalledAppManager>> _mockFileBackedConfigIAValidator;
        private readonly Mock<FileBackedConfigValidator<FileBackedStateManager<string>>> _mockFileBackedConfigStateValidator;
        private readonly Mock<ISmartThingsAPIHelper> _mockSmartThingsAPIHelper;
        private readonly IInstalledAppManager _installedAppManager;
        private readonly IStateManager<string> _stateManager;

        public FileBackedTests(ITestOutputHelper output)
        {
            _mockIALogger = new Mock<ILogger<IInstalledAppManager>>();
            _mockStateLogger = new Mock<ILogger<IStateManager<string>>>();
            _mockIALogger.Setup(log => log.Log(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                null,
                It.IsAny<Func<object, Exception, string>>()))
                    .Callback<Microsoft.Extensions.Logging.LogLevel,
                        EventId,
                        object,
                        Exception,
                        Func<object, Exception, string>>((logLevel, e, state, ex, f) =>
                        {
                            output.WriteLine($"{logLevel} logged: \"{state}\"");
                        });
            _mockStateLogger.Setup(log => log.Log(It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                null,
                It.IsAny<Func<object, Exception, string>>()))
                .Callback<Microsoft.Extensions.Logging.LogLevel,
                    EventId,
                    object,
                    Exception,
                    Func<object, Exception, string>>((logLevel, e, state, ex, f) =>
                    {
                        output.WriteLine($"{logLevel} logged: \"{state}\"");
                    });

            _mockSmartThingsAPIHelper = new Mock<ISmartThingsAPIHelper>();
            _mockSmartThingsAPIHelper.Setup(api => api.RefreshTokensAsync(It.IsAny<InstalledAppInstance>()))
                .Returns(() =>
                {
                    return Task.FromResult<InstalledAppInstance>(CommonUtils.GetValidInstalledAppInstance());
                });

            var iaConfig = new FileBackedConfig<FileBackedInstalledAppManager>()
            {
                BackingStorePath = "data/ia.store"
            };

            var stateConfig = new FileBackedConfig<FileBackedStateManager<string>>()
            {
                BackingStorePath = "data/state.store"
            };

            _mockIAOptions = new Mock<IOptions<FileBackedConfig<FileBackedInstalledAppManager>>>();
            _mockIAOptions.Setup(opt => opt.Value)
                .Returns(iaConfig);

            _mockStateOptions = new Mock<IOptions<FileBackedConfig<FileBackedStateManager<string>>>>();
            _mockStateOptions.Setup(opt => opt.Value)
                .Returns(stateConfig);

            var mockIAFileData = new MockFileData(JsonConvert.SerializeObject(CommonUtils.GetIACache()));

            var mockStateFileData = new MockFileData(JsonConvert.SerializeObject(CommonUtils.GetStateCache()));

            var mockFileSystem = new MockFileSystem();

            mockFileSystem.AddFile("data/ia.store", mockIAFileData);

            mockFileSystem.AddFile("data/state.store", mockStateFileData);

            _mockFileBackedConfigIAValidator = new Mock<FileBackedConfigValidator<FileBackedInstalledAppManager>>();

            _installedAppManager = new FileBackedInstalledAppManager(_mockIALogger.Object,
                _mockSmartThingsAPIHelper.Object,
                _mockIAOptions.Object,
                _mockFileBackedConfigIAValidator.Object,
                mockFileSystem);

            _mockFileBackedConfigStateValidator = new Mock<FileBackedConfigValidator<FileBackedStateManager<string>>>();

            _stateManager = new FileBackedStateManager<string>(_mockStateLogger.Object,
                _mockStateOptions.Object,
                _mockFileBackedConfigStateValidator.Object,
                mockFileSystem);
        }

        public static IEnumerable<object[]> ValidInstalledAppInstance()
        {
            var installedApp = CommonUtils.GetValidInstalledAppInstance();

            return new List<object[]>
            {
                new object[] { installedApp }
            };
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IAStoreInstalledAppAsyncc_ShouldNotError(InstalledAppInstance installedApp)
        {
            await _installedAppManager.StoreInstalledAppAsync(installedApp);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IAGetInstalledAppAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var result = await _installedAppManager.GetInstalledAppAsync(installedApp.InstalledAppId);
            Assert.NotNull(result);
            Assert.Equal(installedApp.InstalledAppId, result.InstalledAppId);
            Assert.Equal(installedApp.InstalledLocation, result.InstalledLocation);
        }

        [Fact]
        public async Task IAPersistCacheAsync_ShouldNotError()
        {
            await _installedAppManager.PersistCacheAsync();
        }

        [Fact]
        public async Task IARefreshAllInstalledAppTokensAsync_ShouldNotError()
        {
            await _installedAppManager.RefreshAllInstalledAppTokensAsync();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IARefreshTokensAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            await _installedAppManager.RefreshTokensAsync(installedApp);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task IARemoveInstalledAppAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            await _installedAppManager.LoadCacheAsync();
            await _installedAppManager.RemoveInstalledAppAsync(installedApp.InstalledAppId);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task StateGetStateAsync_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var result = await _stateManager.GetStateAsync(installedApp.InstalledAppId);
            Assert.NotNull(result);
            Assert.Equal(CommonUtils.GetStateObject(), result);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task StateRemoveStateAsync_ShouldNotError(InstalledAppInstance installedApp)
        {
            await _stateManager.RemoveStateAsync(installedApp.InstalledAppId);
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public async Task StateStoreStateAsync_ShouldNotErrorAndShouldNotify(InstalledAppInstance installedApp)
        {
            var observer = new StateObserver(_mockStateLogger.Object);
            _stateManager.Subscribe(observer);
            await _stateManager.StoreStateAsync(installedApp.InstalledAppId, CommonUtils.GetStateObject());
        }
    }
}
