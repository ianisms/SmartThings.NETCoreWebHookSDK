#region Copyright
// <copyright file="AzureStorageBackedTests.cs" company="Ian N. Bennett">
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

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using FluentValidation;
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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class AzureStorageBackedTests
    {
        private readonly Mock<ILogger<IInstalledAppManager>> _mockIALogger;
        private readonly Mock<ILogger<IStateManager<string>>> _mockStateLogger;
        private readonly Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>>> _mockIAOptions;
        private readonly Mock<AzureStorageBackedConfigValidator<AzureStorageBackedInstalledAppManager>> _mockAzureStorageBackedIAConfigValidator;
        private readonly Mock<AzureStorageBackedConfigWithClientValidator<AzureStorageBackedInstalledAppManager>> _mockAzureStorageBackedIAConfigWithClientValidator;
        private readonly Mock<AzureStorageBackedConfigValidator<AzureStorageBackedStateManager<string>>> _mockAzureStorageBackedStateConfigValidator;
        private readonly Mock<AzureStorageBackedConfigWithClientValidator<AzureStorageBackedStateManager<string>>> _mockAzureStorageBackedStateConfigWithClientValidator;
        private readonly Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<string>>>> _mockStateOptions;

        private readonly Mock<ISmartThingsAPIHelper> _mockSmartThingsAPIHelper;
        private readonly Mock<BlobServiceClient> _mockIABlobServiceClient;
        private readonly Mock<BlobServiceClient> _mockStateBlobServiceClient;
        private readonly IInstalledAppManager _installedAppManager;
        private readonly IStateManager<string> _stateManager;

        public AzureStorageBackedTests(ITestOutputHelper output)
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

            var iaBlobStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(CommonUtils.GetIACache())));
            iaBlobStream.Seek(0, SeekOrigin.Begin);

            var iaBlobStreamInfo = BlobsModelFactory.BlobDownloadInfo(DateTimeOffset.Now,
                0,
                BlobType.Block,
                null,
                null,
                null,
                null,
                null,
                null,
                CopyStatus.Success,
                null,
                LeaseDurationType.Infinite,
                null,
                LeaseState.Available,
                null,
                LeaseStatus.Unlocked,
                null,
                null,
                default,
                0,
                null,
                false,
                null,
                null,
                null,
                100,
                null,
                null,
                null,
                iaBlobStream);

            var mockTrueResponse = new Mock<Azure.Response<bool>>();
            mockTrueResponse.Setup(m => m.Value).Returns(true);

            var mockFalseResponse = new Mock<Azure.Response<bool>>();
            mockFalseResponse.Setup(m => m.Value).Returns(false);

            var mockIABlobDownloadInfoResponse = new Mock<Azure.Response<BlobDownloadInfo>>();
            mockIABlobDownloadInfoResponse.Setup(m => m.Value)
                .Returns(iaBlobStreamInfo);

            var mockIABlobClient = new Mock<BlobClient>();
            mockIABlobClient.Setup(m => m.DownloadAsync())
                .ReturnsAsync(mockIABlobDownloadInfoResponse.Object);
            mockIABlobClient.Setup(m => m.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTrueResponse.Object);

            var mockIABlobContainerClient = new Mock<BlobContainerClient>();
            mockIABlobContainerClient.Setup(m => m.GetBlobClient(It.IsAny<string>()))
                .Returns(mockIABlobClient.Object);

            _mockIABlobServiceClient = new Mock<BlobServiceClient>();
            _mockIABlobServiceClient.Setup(m => m.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockIABlobContainerClient.Object);

            var stateBlobStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(CommonUtils.GetStateCache())));
            stateBlobStream.Seek(0, SeekOrigin.Begin);

            var stateBlobStreamInfo = BlobsModelFactory.BlobDownloadInfo(DateTimeOffset.Now,
                0,
                BlobType.Block,
                null,
                null,
                null,
                null,
                null,
                null,
                CopyStatus.Success,
                null,
                LeaseDurationType.Infinite,
                null,
                LeaseState.Available,
                null,
                LeaseStatus.Unlocked,
                null,
                null,
                default,
                0,
                null,
                false,
                null,
                null,
                null,
                100,
                null,
                null,
                null,
                stateBlobStream);

            var mockStateBlobDownloadInfoResponse = new Mock<Azure.Response<BlobDownloadInfo>>();
            mockStateBlobDownloadInfoResponse.Setup(m => m.Value)
                .Returns(stateBlobStreamInfo);

            var mockStateBlobClient = new Mock<BlobClient>();
            mockStateBlobClient.Setup(m => m.DownloadAsync())
                .ReturnsAsync(mockStateBlobDownloadInfoResponse.Object);
            mockStateBlobClient.Setup(m => m.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTrueResponse.Object);

            var mockStateBlobContainerClient = new Mock<BlobContainerClient>();
            mockStateBlobContainerClient.Setup(m => m.GetBlobClient(It.IsAny<string>()))
                .Returns(mockStateBlobClient.Object);

            _mockStateBlobServiceClient = new Mock<BlobServiceClient>();
            _mockStateBlobServiceClient.Setup(m => m.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockStateBlobContainerClient.Object);

            var iaConfig = new AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>()
            {
                CacheBlobName = "Blob",
                ConnectionString = "ConnString",
                ContainerName = "Blobs"
            };

            var stateConfig = new AzureStorageBackedConfig<AzureStorageBackedStateManager<string>>()
            {
                CacheBlobName = "Blob",
                ConnectionString = "ConnString",
                ContainerName = "Blobs"
            };

            _mockIAOptions = new Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedInstalledAppManager>>>();
            _mockIAOptions.Setup(opt => opt.Value)
                .Returns(iaConfig);

            _mockStateOptions = new Mock<IOptions<AzureStorageBackedConfig<AzureStorageBackedStateManager<string>>>>();
            _mockStateOptions.Setup(opt => opt.Value)
                .Returns(stateConfig);

            _mockAzureStorageBackedIAConfigValidator = new Mock<AzureStorageBackedConfigValidator<AzureStorageBackedInstalledAppManager>>();
            _mockAzureStorageBackedIAConfigWithClientValidator = new Mock<AzureStorageBackedConfigWithClientValidator<AzureStorageBackedInstalledAppManager>>();

            _installedAppManager = new AzureStorageBackedInstalledAppManager(_mockIALogger.Object,
                _mockSmartThingsAPIHelper.Object,
                _mockIAOptions.Object,
                _mockAzureStorageBackedIAConfigWithClientValidator.Object,
                _mockIABlobServiceClient.Object);

            _installedAppManager.LoadCacheAsync().Wait();

            _mockAzureStorageBackedStateConfigValidator = new Mock<AzureStorageBackedConfigValidator<AzureStorageBackedStateManager<string>>>();
            _mockAzureStorageBackedStateConfigWithClientValidator = new Mock<AzureStorageBackedConfigWithClientValidator<AzureStorageBackedStateManager<string>>>();


            _stateManager = new AzureStorageBackedStateManager<string>(_mockStateLogger.Object,
                _mockStateOptions.Object,
                _mockAzureStorageBackedStateConfigWithClientValidator.Object,
                _mockStateBlobServiceClient.Object);

            _stateManager.LoadCacheAsync().Wait();
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

        [Fact]
        public async Task AzureStorageBackedConfigValidator_Should_Not_Throw()
        {
            var config = new AzureStorageBackedConfig<string>()
            {
                CacheBlobName = "foo",
                ConnectionString = "foo",
                ContainerName = "foo"
            };

            var validator = new AzureStorageBackedConfigValidator<string>();

            await validator.ValidateAndThrowAsync(config);
        }

        [Theory]
        [InlineData(null, "foo", "foo", "of null CacheBlobName")]
        [InlineData("foo", null, "foo", "of null ConnectionString")]
        [InlineData("foo", "foo", null, "of null ContainerName")]
        [InlineData("", "foo", "foo", "of empty CacheBlobName")]
        [InlineData("foo", "", "foo", "of empty ConnectionString")]
        [InlineData("foo", "foo", "", "of empty ContainerName")]
        public void AzureStorageBackedConfigValidator_Should_Throw(string cacheBlobName,
            string conmnectionString,
            string containerName,
            string description)
        {
            var config = new AzureStorageBackedConfig<string>()
            {
                CacheBlobName = cacheBlobName,
                ConnectionString = conmnectionString,
                ContainerName = containerName
            };

            var validator = new AzureStorageBackedConfigValidator<string>();

            Func<Task> act = async () =>
            {
                await validator.ValidateAndThrowAsync(config);
            };
            act.Should().Throw<ValidationException>(description);
        }
    }
}
