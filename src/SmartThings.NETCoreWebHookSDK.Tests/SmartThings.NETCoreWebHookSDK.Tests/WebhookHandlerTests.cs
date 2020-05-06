#region Copyright
// <copyright file="InstalledAppTokenManagerTests.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.State;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class WebhookHandlerTests
    {
        #region MockHandlers 
        public class MockState
        {
            public string InstalledAppId { get; set; }
            public bool IsAppEnabled { get; set; }
            public IList<LightSwitch> LightSwitches { get; set; }

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        class MockConfigWebhookHandler : ConfigWebhookHandler
        {
            public MockConfigWebhookHandler(ILogger<ConfigWebhookHandler> logger)
                : base(logger)
            {
            }

            private static readonly dynamic initResponse = JObject.Parse(@"
            {
                'configurationData': {
                    'initialize': {
                        'id': 'app',
                        'name': 'My App',
                        'permissions': ['r:devices:*','r:locations:*'],
                        'firstPageId': '1'
                    }
                }
            }");

            private static readonly dynamic pageOneResponse = JObject.Parse(@"
            {
                'configurationData': {
                    'page': {
                        'pageId': '1',
                        'name': 'Configure My App',
                        'nextPageId': null,
                        'previousPageId': null,
                        'complete': true,
                        'sections' : [
                            {
                                'name': 'basics',
                                'settings' : [
                                    {
                                        'id': 'isAppEnabled',
                                        'name': 'Enabled App?',
                                        'description': 'Easy toggle to enable/disable the app',
                                        'type': 'BOOLEAN',
                                        'required': true,
                                        'defaultValue': true,
                                        'multiple': false
                                    },                       
                                    {
                                        'id': 'switches',
                                        'name': 'Which Light Switch(es)?',
                                        'description': 'The switch(es) to turn on/off on arrival.',
                                        'type': 'DEVICE',
                                        'required': false,
                                        'multiple': true,
                                        'capabilities': ['switch'],
                                        'permissions': ['r', 'x']
                                    }
                                ]
                            }
                        ]
                    }
                }
            }");

            public override dynamic Initialize(dynamic request)
            {
                return initResponse;
            }

            public override dynamic Page(dynamic request)
            {
                return pageOneResponse;
            }
        }

        class MockInstallUpdateDataHandler : InstallUpdateWebhookHandler
        {
            private readonly IStateManager<MockState> stateManager;

            public MockInstallUpdateDataHandler(ILogger<IInstallUpdateWebhookHandler> logger,
                IOptions<SmartAppConfig> options,
                IInstalledAppManager installedAppManager,
                ISmartThingsAPIHelper smartThingsAPIHelper,
                IStateManager<MockState> stateManager)
                : base(logger, options, installedAppManager, smartThingsAPIHelper)
            {
                _ = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
                this.stateManager = stateManager;
            }

            private async Task SubscribeToDeviceEventAsync(InstalledAppInstance installedApp,
                dynamic deviceConfig)
            {
                _ = installedApp ??
                    throw new ArgumentNullException(nameof(installedApp));
                _ = deviceConfig ??
                    throw new ArgumentNullException(nameof(deviceConfig));

                var resp = await SmartThingsAPIHelper.SubscribeToDeviceEventAsync(
                    installedApp,
                    deviceConfig);

                var body = await resp.Content.ReadAsStringAsync();
                dynamic subscriptionResp = JObject.Parse(body);

                _ = subscriptionResp.id ??
                    throw new InvalidOperationException("subscriptionResp.id is null!");
            }

            private async Task LoadLightSwitchesAsync(InstalledAppInstance installedApp,
                MockState state,
                dynamic data,
                bool shouldSubscribeToEvents = true)
            {
                Logger.LogInformation("Loading lightSwitches...");
                state.LightSwitches = new List<LightSwitch>();

                _ = installedApp ??
                    throw new ArgumentNullException(nameof(installedApp));
                _ = state ??
                    throw new ArgumentNullException(nameof(state));
                _ = data ??
                    throw new ArgumentNullException(nameof(data));

                var lightSwitches = data.installedApp.config.switches;
                if (lightSwitches != null)
                {
                    var index = 0;

                    foreach (var device in lightSwitches)
                    {
                        dynamic deviceConfig = device.deviceConfig;
                        var deviceId = deviceConfig.deviceId.Value;
                        deviceConfig.capability = "switch";
                        deviceConfig.attribute = "switch";
                        deviceConfig.stateChangeOnly = true;
                        deviceConfig.value = "*";
                        deviceConfig.subscriptionName = $"MySwitches{index}";
                        index++;

                        var deviceTasks = new Task<dynamic>[] {
                        SmartThingsAPIHelper.GetDeviceDetailsAsync(
                            installedApp,
                            deviceId),
                        SmartThingsAPIHelper.GetDeviceStatusAsync(
                            installedApp,
                            deviceId)
                    };

                        Task.WaitAll(deviceTasks);

                        var ls = LightSwitch.SwitchFromDynamic(
                            deviceTasks[0].Result,
                            deviceTasks[1].Result);

                        state.LightSwitches.Add(ls);

                        if (shouldSubscribeToEvents)
                        {
                            Task.Run(() => SubscribeToDeviceEventAsync(installedApp, deviceConfig).ConfigureAwait(false));
                        }
                    }
                }

                Logger.LogInformation($"Loaded {state.LightSwitches.Count} lightSwitches...");
            }

            public async Task HandleInstallUpdateDataAsync(InstalledAppInstance installedApp,
                dynamic data,
                bool shouldSubscribeToEvents = true)
            {
                _ = installedApp ??
                    throw new ArgumentNullException(nameof(installedApp));
            }

            public override async Task HandleUpdateDataAsync(InstalledAppInstance installedApp,
            dynamic data,
                bool shouldSubscribeToEvents = true)
            {
                _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
                _ = data ?? throw new ArgumentNullException(nameof(data));
            }

            public override async Task HandleInstallDataAsync(InstalledAppInstance installedApp,
                dynamic data,
                bool shouldSubscribeToEvents = true)
            {
                _ = installedApp ?? throw new ArgumentNullException(nameof(installedApp));
                _ = data ?? throw new ArgumentNullException(nameof(data));
            }
        }

        class MockEventWebhookHandler : EventWebhookHandler
        {
            private readonly IStateManager<MockState> stateManager;
            private readonly IInstallUpdateWebhookHandler installUpdateHandler;

            public MockEventWebhookHandler(ILogger<EventWebhookHandler> logger,
                IInstalledAppManager installedAppManager,
                IStateManager<MockState> stateManager,
                IInstallUpdateWebhookHandler installUpdateHandler)
                : base(logger, installedAppManager)
            {
                this.stateManager = stateManager;
                this.installUpdateHandler = installUpdateHandler;
            }

            public override void ValidateRequest(dynamic request)
            {
                base.ValidateRequest((JObject)request);
            }

            public override async Task HandleEventDataAsync(InstalledAppInstance installedApp,
                dynamic eventData)
            {
                _ = installedApp ??
                    throw new ArgumentNullException(nameof(installedApp));
                _ = eventData ??
                    throw new ArgumentNullException(nameof(eventData));
            }

            private async Task HandleDeviceEventAsync(MockState state, dynamic deviceEvent)
            {
                _ = state ??
                    throw new ArgumentNullException(nameof(state));
                _ = deviceEvent ??
                    throw new ArgumentNullException(nameof(deviceEvent));
                _ = deviceEvent.subscriptionName ??
                    throw new ArgumentException($"deviceEvent.subscriptionName is null!",
                    nameof(deviceEvent));
            }
        }

        class MockUninstallWebhookHandler : UninstallWebhookHandler
        {
            private readonly IStateManager<MockState> stateManager;

            public MockUninstallWebhookHandler(ILogger<UninstallWebhookHandler> logger,
                IInstalledAppManager installedAppManager,
                IStateManager<MockState> stateManager)
                : base(logger, installedAppManager)
            {
                _ = stateManager ??
                    throw new ArgumentNullException(nameof(stateManager));
                this.stateManager = stateManager;
            }

            public override async Task HandleUninstallDataAsync(dynamic uninstallData)
            {
                var installedAppId = (string)uninstallData.installedApp.installedAppId;
                await stateManager.RemoveStateAsync(installedAppId).ConfigureAwait(false);
            }
        }
        #endregion MockHandlers

        private readonly Mock<IInstalledAppManager> mockIAManager;
        private readonly Mock<IStateManager<MockState>> mockStateManager;
        private readonly Mock<IOptions<SmartAppConfig>> mockSmartAppOptions;
        private readonly Mock<ISmartThingsAPIHelper> mockSmartThingsAPIHelper;
        private readonly MockState mockState;
        private readonly IPingWebhookHandler pingWebhookHandler;
        private readonly IConfirmationWebhookHandler confirmationWebhookHandler;
        private readonly IInstallUpdateWebhookHandler installUpdateWebhookHandler;
        private readonly IConfigWebhookHandler configWebhookHandler;
        private readonly IEventWebhookHandler eventWebhookHandler;
        private readonly IUninstallWebhookHandler uninstallWebhookHandler;
        private readonly IOAuthWebhookHandler oAuthWebhookHandler;
        private readonly IRootWebhookHandler rootWebhookHandler;

        public WebhookHandlerTests()
        {
            mockIAManager = new Mock<IInstalledAppManager>();
            mockIAManager.Setup(mgr => mgr.GetInstalledAppAsync(It.IsAny<string>()))
                .Returns(() =>
                {
                    return Task.FromResult<InstalledAppInstance>(CommonUtils.GetValidInstalledAppInstance());
                });


            mockState = new MockState()
            {
                InstalledAppId = CommonUtils.GetValidInstalledAppInstance().InstalledAppId,
                IsAppEnabled = true,
                LightSwitches = new List<LightSwitch>()
                {
                    new LightSwitch()
                    {
                        CurrentState = SwitchState.Off,
                        Id = "9A047F4E-B81B-4865-AD19-76AC194F4AB6",
                        Label = "The Switch"
                    }
                }
            };

            mockStateManager = new Mock<IStateManager<MockState>>();

            mockStateManager.Setup(mgr => mgr.GetStateAsync(It.IsAny<string>()))
                .Returns(() =>
                {
                    return Task.FromResult<MockState>(mockState);
                });

            mockStateManager.Setup(mgr => mgr.RemoveStateAsync(It.IsAny<string>()))
                .Returns(() =>
                {
                    return Task.FromResult<MockState>(mockState);
                });

            var smartAppConfig = new SmartAppConfig()
            {
                SmartAppClientId = "CLIENT_ID",
                SmartAppClientSecret = "CLIENT_SECRET",
                PAT = "PAT"
            };

            mockSmartAppOptions = new Mock<IOptions<SmartAppConfig>>();
            mockSmartAppOptions.Setup(opt => opt.Value)
                .Returns(smartAppConfig);

            mockSmartThingsAPIHelper = new Mock<ISmartThingsAPIHelper>();
            mockSmartThingsAPIHelper.Setup(api => api.GetDeviceDetailsAsync(It.IsAny<InstalledAppInstance>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    var respoonse = JObject.Parse(@"
                        {
                            ""deviceId"": ""9A047F4E-B81B-4865-AD19-76AC194F4AB6"",
                            ""label"": ""The Switch""
                        }
                    ");
                    return Task.FromResult<dynamic>(respoonse);
                });

            mockSmartThingsAPIHelper.Setup(api => api.GetDeviceStatusAsync(It.IsAny<InstalledAppInstance>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    var respoonse = JObject.Parse(@"
                        {
                            ""components"": {
                                ""main"": {
                                    ""switch"": {
                                        ""switch"": {
                                            ""value"": ""on""
                                        }
                                    },
                                    ""switchLevel"": {
                                        ""level"": {
                                            ""value"": 90
                                        }
                                    }
                                }
                            }
                        }
                    ");
                    return Task.FromResult<dynamic>(respoonse);
                });

            mockSmartThingsAPIHelper.Setup(api => api.SubscribeToDeviceEventAsync(It.IsAny<InstalledAppInstance>(), It.IsAny<object>()))
                .Returns(() =>
                {
                    var respoonse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    return Task.FromResult<HttpResponseMessage>(respoonse);
                });

            var mockConfirmationHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockConfirmationHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"message\":\"ok\"}"),
               });

            var mockConfirmationHttpClientFactory = mockConfirmationHttpMessageHandler.CreateClientFactory();

            pingWebhookHandler = new PingWebhookHandler(
                new Mock<ILogger<IPingWebhookHandler>>().Object);
            confirmationWebhookHandler = new ConfirmationWebhookHandler(
                new Mock<ILogger<IConfirmationWebhookHandler>>().Object,
                mockSmartAppOptions.Object,
                mockConfirmationHttpClientFactory);
            configWebhookHandler = new MockConfigWebhookHandler(
                new Mock<ILogger<ConfigWebhookHandler>>().Object);
            installUpdateWebhookHandler = new MockInstallUpdateDataHandler(
                new Mock<ILogger<IInstallUpdateWebhookHandler>>().Object,
                mockSmartAppOptions.Object,
                mockIAManager.Object,
                mockSmartThingsAPIHelper.Object,
                mockStateManager.Object);
            eventWebhookHandler = new MockEventWebhookHandler(
                new Mock<ILogger<EventWebhookHandler>>().Object,
                mockIAManager.Object,
                mockStateManager.Object,
                installUpdateWebhookHandler);
            uninstallWebhookHandler = new MockUninstallWebhookHandler(
                new Mock<ILogger<UninstallWebhookHandler>>().Object,
                mockIAManager.Object,
                mockStateManager.Object);
            oAuthWebhookHandler = new OAuthWebhookHandler(
                new Mock<ILogger<IOAuthWebhookHandler>>().Object);
            rootWebhookHandler = new RootWebhookHandler(
                new Mock<ILogger<IRootWebhookHandler>>().Object,
                pingWebhookHandler,
                confirmationWebhookHandler,
                configWebhookHandler,
                installUpdateWebhookHandler,
                eventWebhookHandler,
                oAuthWebhookHandler,
                uninstallWebhookHandler,
                new Mock<ICryptoUtils>().Object);
        }

        [Fact]
        public void PingWebhookHandler_HandleRequest_ShouldReturnExpectedResult()
        {
            dynamic response = pingWebhookHandler.HandleRequest(JObject.Parse(@"
                {
                    ""lifecycle"": ""PING"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""pingData"": {
                        ""challenge"": ""1a904d57-4fab-4b15-a11e-1c4bfe7cb502""
                    }
                }
            "));
            Assert.Equal("1a904d57-4fab-4b15-a11e-1c4bfe7cb502", (string)response.pingData.challenge);
        }

        [Fact]
        public async Task ConfirmationWebhookHandler_HandleRequestAsync_ShouldReturnExpectedResult()
        {
            dynamic response = await confirmationWebhookHandler.HandleRequestAsync(JObject.Parse(@"
                {
                    ""lifecycle"": ""CONFIRMATION"",
                    ""executionId"": ""8F8FA33E-2A5B-4BC5-826C-4B2AB73FE9DD"",
                    ""appId"": ""fd9949ee-a3bf-4069-b4b3-3e9c1c922e29"",
                    ""locale"": ""en"",
                    ""version"": ""0.1.0"",
                    ""confirmationData"": {
                        ""appId"": ""fd9949ee-a3bf-4069-b4b3-3e9c1c922e29"",
                        ""confirmationUrl"": ""https://localhost.com""
                    },
                    ""settings"": {}
                }
            "));
            Assert.Equal("https://localhost.com", (string)response.targetUrl);
        }

        [Fact]
        public void ConfigWebHookHandler_handleRequest_ShouldNotReturnNull()
        {
            dynamic response = configWebhookHandler.HandleRequest(JObject.Parse(@"
                {
                    ""lifecycle"": ""CONFIGURATION"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""configurationData"": {
                        ""installedAppId"": ""string"",
                        ""phase"": ""INITIALIZE"",
                        ""pageId"": ""string"",
                        ""previousPageId"": ""string"",
                        ""config"": {
                            ""property1"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""31192dc9-eb45-4d90-b606-21e9b66d8c2b"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""property2"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""31192dc9-eb45-4d90-b606-21e9b66d8c2b"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            "));
            Assert.NotNull(response);
        }

        [Fact]
        public void ConfigWebHookHandler_Initialize_ShouldNotReturnNull()
        {
            dynamic response = configWebhookHandler.Initialize(JObject.Parse("{}"));
            Assert.NotNull(response);
        }

        [Fact]
        public void ConfigWebHookhandler_Page_ShouldNotReturnNull()
        {
            dynamic response = configWebhookHandler.Page(JObject.Parse("{}"));
            Assert.NotNull(response);
        }

        [Fact]
        public void InstallUpdateWebhookHandler_ValidateRequestAsync_ShouldNotError()
        {
            installUpdateWebhookHandler.ValidateRequest(Lifecycle.Install, JObject.Parse(@"
                {
                    ""lifecycle"": ""INSTALL"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""installData"": {
                        ""authToken"": ""string"",
                        ""refreshToken"": ""string"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            "));

            installUpdateWebhookHandler.ValidateRequest(Lifecycle.Update, JObject.Parse(@"
             {
                ""lifecycle"": ""UPDATE"",
                ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                ""locale"": ""en"",
                ""version"": ""1.0.0"",
                ""updateData"": {
                    ""authToken"": ""string"",
                    ""refreshToken"": ""string"",
                    ""installedApp"": {
                        ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                        ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                        ""config"": {
                            ""contactSensor"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""lightSwitch"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""minutes"": [
                                {
                                    ""valueType"": ""STRING"",
                                    ""stringConfig"": {
                                        ""value"": ""5""
                                    }
                                }
                            ],
                        },
                        ""permissions"": [
                            ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                            ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                            ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                        ]
                    },
                    ""previousConfig"": {
                        ""contactSensor"": [
                            {
                                ""valueType"": ""DEVICE"",
                                ""deviceConfig"": {
                                    ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                    ""componentId"": ""main""
                                }
                            }
                        ],
                        ""lightSwitch"": [
                            {
                                ""valueType"": ""DEVICE"",
                                ""deviceConfig"": {
                                    ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                    ""componentId"": ""main""
                                }
                            }
                        ],
                        ""minutes"": [
                            {
                                ""valueType"": ""STRING"",
                                ""stringConfig"": {
                                    ""value"": ""5""
                                }
                            }
                        ],
                    },
                    ""previousPermissions"": [
                        ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                        ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                        ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                    ]
                },
                ""settings"": {
                    ""property1"": ""string"",
                    ""property2"": ""string""
                }
            }
            "));
        }

        [Fact]
        public async Task InstallUpdateWebhookHandler_HandleInstallDataAsync_ShouldNotError()
        {
            await installUpdateWebhookHandler.HandleInstallDataAsync(CommonUtils.GetValidInstalledAppInstance(),
                JObject.Parse("{}"));
        }

        [Fact]
        public async Task InstallUpdateWebhookHandler_HandleInstallRequestAsync_ShouldNotError()
        {
            await installUpdateWebhookHandler.HandleRequestAsync(Lifecycle.Install,
                JObject.Parse(@"
                    {
                        ""lifecycle"": ""INSTALL"",
                        ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                        ""locale"": ""en"",
                        ""version"": ""1.0.0"",
                        ""installData"": {
                            ""authToken"": ""string"",
                            ""refreshToken"": ""string"",
                            ""installedApp"": {
                                ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                                ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                                ""config"": {
                                    ""contactSensor"": [
                                        {
                                            ""valueType"": ""DEVICE"",
                                            ""deviceConfig"": {
                                                ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                                ""componentId"": ""main""
                                            }
                                        }
                                    ],
                                    ""lightSwitch"": [
                                        {
                                            ""valueType"": ""DEVICE"",
                                            ""deviceConfig"": {
                                                ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                                ""componentId"": ""main""
                                            }
                                        }
                                    ],
                                    ""minutes"": [
                                        {
                                            ""valueType"": ""STRING"",
                                            ""stringConfig"": {
                                                ""value"": ""5""
                                            }
                                        }
                                    ],
                                },
                                ""permissions"": [
                                    ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                    ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                    ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                                ]
                            }
                        },
                        ""settings"": {
                            ""property1"": ""string"",
                            ""property2"": ""string""
                        }
                    }
                "));
        }

        [Fact]
        public async Task InstallUpdateWebhookHandler_HandleUpdateRequestAsync_ShouldNotError()
        {
            await installUpdateWebhookHandler.HandleRequestAsync(Lifecycle.Update,
                JObject.Parse(@"
                    {
                    ""lifecycle"": ""UPDATE"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""updateData"": {
                        ""authToken"": ""string"",
                        ""refreshToken"": ""string"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        },
                        ""previousConfig"": {
                            ""contactSensor"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""lightSwitch"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""minutes"": [
                                {
                                    ""valueType"": ""STRING"",
                                    ""stringConfig"": {
                                        ""value"": ""5""
                                    }
                                }
                            ],
                        },
                        ""previousPermissions"": [
                            ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                            ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                            ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                        ]
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
                "));
        }

        [Fact]
        public async Task InstallUpdateWebhookHandler_HandleUpdateDataAsync_ShouldNotError()
        {
            await installUpdateWebhookHandler.HandleUpdateDataAsync(CommonUtils.GetValidInstalledAppInstance(),
                JObject.Parse("{}"));
        }

        [Fact]
        public async Task EventWebhookHandler_HandleRequest_ShouldNotReturnNull()
        {
            dynamic response = await eventWebhookHandler.HandleRequestAsync(JObject.Parse(@"
                {
                    ""lifecycle"": ""EVENT"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""eventData"": {
                        ""authToken"": ""f01894ce-013a-434a-b51e-f82126fd72e4"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        },
                        ""events"": [
                            {
                                ""eventType"": ""DEVICE_EVENT"",
                                ""deviceEvent"": {
                                    ""subscriptionName"": ""motion_sensors"",
                                    ""eventId"": ""736e3903-001c-4d40-b408-ff40d162a06b"",
                                    ""locationId"": ""499e28ba-b33b-49c9-a5a1-cce40e41f8a6"",
                                    ""deviceId"": ""6f5ea629-4c05-4a90-a244-cc129b0a80c3"",
                                    ""componentId"": ""main"",
                                    ""capability"": ""motionSensor"",
                                    ""attribute"": ""motion"",
                                    ""value"": ""active"",
                                    ""stateChange"": true
                                }
                            }
                        ]
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            "));

            Assert.NotNull(response);
        }

        [Fact]
        public void EventWebhookHandler_ValidateRequest_ShouldNotError()
        {
            eventWebhookHandler.ValidateRequest(JObject.Parse(@"
                {
                    ""lifecycle"": ""EVENT"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""eventData"": {
                        ""authToken"": ""f01894ce-013a-434a-b51e-f82126fd72e4"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        },
                        ""events"": [
                            {
                                ""eventType"": ""DEVICE_EVENT"",
                                ""deviceEvent"": {
                                    ""subscriptionName"": ""motion_sensors"",
                                    ""eventId"": ""736e3903-001c-4d40-b408-ff40d162a06b"",
                                    ""locationId"": ""499e28ba-b33b-49c9-a5a1-cce40e41f8a6"",
                                    ""deviceId"": ""6f5ea629-4c05-4a90-a244-cc129b0a80c3"",
                                    ""componentId"": ""main"",
                                    ""capability"": ""motionSensor"",
                                    ""attribute"": ""motion"",
                                    ""value"": ""active"",
                                    ""stateChange"": true
                                }
                            }
                        ]
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            "));
        }

        [Fact]
        public async Task EventWebhookHandler_HandleEventDataAsync_ShouldNotError()
        {
            await eventWebhookHandler.HandleEventDataAsync(CommonUtils.GetValidInstalledAppInstance(),
                JObject.Parse(@"
                    {
                        ""authToken"": ""f01894ce-013a-434a-b51e-f82126fd72e4"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        },
                        ""events"": [
                            {
                                ""eventType"": ""DEVICE_EVENT"",
                                ""deviceEvent"": {
                                    ""subscriptionName"": ""MySwitches"",
                                    ""eventId"": ""736e3903-001c-4d40-b408-ff40d162a06b"",
                                    ""locationId"": ""499e28ba-b33b-49c9-a5a1-cce40e41f8a6"",
                                    ""deviceId"": ""9A047F4E-B81B-4865-AD19-76AC194F4AB6"",
                                    ""componentId"": ""main"",
                                    ""capability"": ""switch"",
                                    ""attribute"": ""switch"",
                                    ""value"": ""on"",
                                    ""stateChange"": true
                                }
                            }
                        ]
                    }
                "));
        }

        [Fact]
        public async Task UninstallUpdateWebhookHandler_HandleRequestAsync_ShouldNotError()
        {
            await uninstallWebhookHandler.HandleRequestAsync(JObject.Parse(@"
                {
                    ""lifecycle"": ""UNINSTALL"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""uninstallData"": {
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            "));
        }

        [Fact]
        public void UninstallUpdateWebhookHandler_ValidateRequestAsync_ShouldNotError()
        {
            uninstallWebhookHandler.ValidateRequest(JObject.Parse(@"
                {
                    ""lifecycle"": ""UNINSTALL"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""uninstallData"": {
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            "));
        }

        [Fact]
        public async Task UninstallWebhookHandler_HandleUninstallDataAsync_ShouldNotError()
        {
            await uninstallWebhookHandler.HandleUninstallDataAsync(JObject.Parse(@"
                {
                ""installedApp"": {
                    ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                    ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                    ""config"": {
                        ""contactSensor"": [
                            {
                                ""valueType"": ""DEVICE"",
                                ""deviceConfig"": {
                                    ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                    ""componentId"": ""main""
                                }
                            }
                        ],
                        ""lightSwitch"": [
                            {
                                ""valueType"": ""DEVICE"",
                                ""deviceConfig"": {
                                    ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                    ""componentId"": ""main""
                                }
                            }
                        ],
                        ""minutes"": [
                            {
                                ""valueType"": ""STRING"",
                                ""stringConfig"": {
                                    ""value"": ""5""
                                }
                            }
                        ],
                    },
                    ""permissions"": [
                        ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                        ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                        ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                    ]
                }
            }"));
        }

        [Fact]
        public async Task OAuthWebhookHandler_HandleRequestAsync_ShouldNotReturnNull()
        {
            dynamic response = oAuthWebhookHandler.HandleRequestAsync(JObject.Parse("{}"));
            Assert.NotNull(response);
        }

        [Fact]
        public async Task RootWebhookHandler_HandleRequestAsync_ShouldNotReturnNull()
        {
            // PING
            var mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""PING"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""pingData"": {
                        ""challenge"": ""1a904d57-4fab-4b15-a11e-1c4bfe7cb502""
                    }
                }
            ");

            dynamic response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // CONFIRMATION 
            mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""CONFIRMATION"",
                    ""executionId"": ""8F8FA33E-2A5B-4BC5-826C-4B2AB73FE9DD"",
                    ""appId"": ""fd9949ee-a3bf-4069-b4b3-3e9c1c922e29"",
                    ""locale"": ""en"",
                    ""version"": ""0.1.0"",
                    ""confirmationData"": {
                        ""appId"": ""fd9949ee-a3bf-4069-b4b3-3e9c1c922e29"",
                        ""confirmationUrl"": ""{CONFIRMATION_URL}""
                    },
                    ""settings"": {}
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // CONFIGURATION
            mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""CONFIGURATION"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""configurationData"": {
                        ""installedAppId"": ""string"",
                        ""phase"": ""INITIALIZE"",
                        ""pageId"": ""string"",
                        ""previousPageId"": ""string"",
                        ""config"": {
                            ""property1"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""31192dc9-eb45-4d90-b606-21e9b66d8c2b"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""property2"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""31192dc9-eb45-4d90-b606-21e9b66d8c2b"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // INSTALL
            mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""INSTALL"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""installData"": {
                        ""authToken"": ""string"",
                        ""refreshToken"": ""string"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // UPDATE
            mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""UPDATE"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""updateData"": {
                        ""authToken"": ""string"",
                        ""refreshToken"": ""string"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        },
                        ""previousConfig"": {
                            ""contactSensor"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""lightSwitch"": [
                                {
                                    ""valueType"": ""DEVICE"",
                                    ""deviceConfig"": {
                                        ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                        ""componentId"": ""main""
                                    }
                                }
                            ],
                            ""minutes"": [
                                {
                                    ""valueType"": ""STRING"",
                                    ""stringConfig"": {
                                        ""value"": ""5""
                                    }
                                }
                            ],
                        },
                        ""previousPermissions"": [
                            ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                            ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                            ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                        ]
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // EVENT
            mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""EVENT"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""eventData"": {
                        ""authToken"": ""f01894ce-013a-434a-b51e-f82126fd72e4"",
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        },
                        ""events"": [
                            {
                                ""eventType"": ""DEVICE_EVENT"",
                                ""deviceEvent"": {
                                    ""subscriptionName"": ""motion_sensors"",
                                    ""eventId"": ""736e3903-001c-4d40-b408-ff40d162a06b"",
                                    ""locationId"": ""499e28ba-b33b-49c9-a5a1-cce40e41f8a6"",
                                    ""deviceId"": ""6f5ea629-4c05-4a90-a244-cc129b0a80c3"",
                                    ""componentId"": ""main"",
                                    ""capability"": ""motionSensor"",
                                    ""attribute"": ""motion"",
                                    ""value"": ""active"",
                                    ""stateChange"": true
                                }
                            }
                        ]
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // OAUTH_CALLBACK
            mockRequest = GetMockRequest(@"
                 {
                    ""lifecycle"": ""OAUTH_CALLBACK"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""oAuthCallbackData"": {
                        ""installedAppId"": ""string"",
                        ""urlPath"": ""string""
                    },
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);

            // UNINSTALL
            mockRequest = GetMockRequest(@"
                {
                    ""lifecycle"": ""UNINSTALL"",
                    ""executionId"": ""b328f242-c602-4204-8d73-33c48ae180af"",
                    ""locale"": ""en"",
                    ""version"": ""1.0.0"",
                    ""uninstallData"": {
                        ""installedApp"": {
                            ""installedAppId"": ""d692699d-e7a6-400d-a0b7-d5be96e7a564"",
                            ""locationId"": ""e675a3d9-2499-406c-86dc-8a492a886494"",
                            ""config"": {
                                ""contactSensor"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""e457978e-5e37-43e6-979d-18112e12c961"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""lightSwitch"": [
                                    {
                                        ""valueType"": ""DEVICE"",
                                        ""deviceConfig"": {
                                            ""deviceId"": ""74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                            ""componentId"": ""main""
                                        }
                                    }
                                ],
                                ""minutes"": [
                                    {
                                        ""valueType"": ""STRING"",
                                        ""stringConfig"": {
                                            ""value"": ""5""
                                        }
                                    }
                                ],
                            },
                            ""permissions"": [
                                ""r:devices:e457978e-5e37-43e6-979d-18112e12c961"",
                                ""r:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76"",
                                ""x:devices:74aac3bb-91f2-4a88-8c49-ae5e0a234d76""
                            ]
                        }
                    },
                    ""settings"": {
                        ""property1"": ""string"",
                        ""property2"": ""string""
                    }
                }
            ");

            response = rootWebhookHandler.HandleRequestAsync(mockRequest);
            Assert.NotNull(response);
        }

        private HttpRequest GetMockRequest(string json)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(json);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            return mockRequest.Object;
        }
    }
}
