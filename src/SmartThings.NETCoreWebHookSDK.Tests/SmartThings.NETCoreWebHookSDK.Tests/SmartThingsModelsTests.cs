﻿#region Copyright
// <copyright file="SmartThingsModelsTests.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class SmartThingsModelsTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SmartThingsModelsTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(@"{
            ""locationId"": ""625BF45D-6C40-4C66-964E-42C1CE22E480"",
            ""name"": ""Home"",
            ""countryCode"": ""USA"",
            ""latitude"": ""40.347054"",
            ""longitude"": ""-74.064308"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""F"",
            ""timeZoneId"": ""America/New_York"",
            ""locale"": ""en""
        }")]
        [InlineData(@"{
            ""locationId"": ""625BF45D-6C40-4C66-964E-42C1CE22E480"",
            ""name"": ""Home"",
            ""countryCode"": ""USA"",
            ""latitude"": ""51.5074"",
            ""longitude"": ""0.1278"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""C"",
            ""timeZoneId"": ""Europe/London"",
            ""locale"": ""en""
        }")]
        public void Location_ShouldParseFromDynamic(string locJson)
        {
            dynamic loc = JObject.Parse(locJson);
            var location = Location.LocationFromDynamic(loc);
            Assert.NotNull(location);
        }

        [Theory]
        [InlineData(@"{
            ""locationId"": ""625BF45D-6C40-4C66-964E-42C1CE22E480"",
            ""name"": ""Home"",
            ""countryCode"": ""USA"",
            ""latitude"": ""40.347054"",
            ""longitude"": ""-74.064308"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""F"",
            ""timeZoneId"": ""America/New_York"",
            ""locale"": ""en""
        }",
        "07/24/2020 05:46:00",
        "07/24/2020 05:46:00",
        "07/24/2020 20:20:00")]
        [InlineData(@"{
            ""locationId"": ""625BF45D-6C40-4C66-964E-42C1CE22E480"",
            ""name"": ""Home"",
            ""countryCode"": ""UK"",
            ""latitude"": ""51.5074"",
            ""longitude"": ""0.1278"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""C"",
            ""timeZoneId"": ""Europe/London"",
            ""locale"": ""en""
        }",
        "07/24/2020 05:13:00",
        "07/24/2020 05:13:00",
        "07/24/2020 21:01:00")]
        public void Location_SunriseSunset_ShouldBeCloseToExpected(string locJson,
            string dateVal,
            string expectedSunriseVal,
            string expectedSunsetVal)
        {
            dynamic loc = JObject.Parse(locJson);
            var location = Location.LocationFromDynamic(loc);
            var dt = DateTime.Parse(dateVal).ToUniversalTime();
            var expectedSunrise = DateTime.Parse(expectedSunriseVal);
            var expectedSunset = DateTime.Parse(expectedSunsetVal);

            var sunrise = location.GetSunrise(dt);
            var sunset = location.GetSunset(dt);

            var sunriseDiff = (sunrise - expectedSunrise).Duration();
            var sunsetDiff = (sunset - expectedSunset).Duration();

            var sunriseCt = sunriseDiff.CompareTo(TimeSpan.FromMinutes(5));
            var sunsetCt = sunsetDiff.CompareTo(TimeSpan.FromMinutes(5));

            testOutputHelper.WriteLine($"Expected sunrise: {expectedSunrise}");
            testOutputHelper.WriteLine($"Expected sunset: {expectedSunset}");
            testOutputHelper.WriteLine($"Computed sunrise: {sunrise}");
            testOutputHelper.WriteLine($"Computed sunset: {sunset}");
            testOutputHelper.WriteLine($"Computed sunrise diff: {sunriseDiff}");
            testOutputHelper.WriteLine($"Computed sunset diff: {sunsetDiff}");

            Assert.True(sunriseCt < 1);
            Assert.True(sunsetCt < 1);
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
        public void AccelerationSensor_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new AccelerationSensor()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = AccelerationState.Active
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""accelerationSensor"": {{
                            ""acceleration"": {{
                                ""value"": ""active""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = AccelerationSensor.AccelerationSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""accelerationSensor"": {{
                            ""acceleration"": {{
                                ""value"": ""inactive""
                            }}
                        }}
                    }}
                }}
            }}";

            expectedResult.CurrentState = AccelerationState.InActive;

            device = JObject.Parse(deviceJson);
            status = JObject.Parse(statusJson);

            result = AccelerationSensor.AccelerationSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""accelerationSensor"": {{
                            ""acceleration"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            expectedResult.CurrentState = AccelerationState.Unknown;

            device = JObject.Parse(deviceJson);
            status = JObject.Parse(statusJson);

            result = AccelerationSensor.AccelerationSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();

            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void AirQualitySensor_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new AirQualitySensor()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentValue = 1
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""airQualitySensor"": {{
                            ""airQuality"": {{
                                ""value"": ""1""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = AirQualitySensor.AirQualitySensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void CarbonMonoxideDetector_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new CarbonMonoxideDetector()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = CarbonMonoxideState.Clear
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""carbonMonoxideDetector"": {{
                            ""carbonMonoxide"": {{
                                ""value"": ""clear""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = CarbonMonoxideDetector.CarbonMonoxideDetectorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = CarbonMonoxideState.Detected;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""carbonMonoxideDetector"": {{
                            ""carbonMonoxide"": {{
                                ""value"": ""detected""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = CarbonMonoxideDetector.CarbonMonoxideDetectorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = CarbonMonoxideState.Tested;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""carbonMonoxideDetector"": {{
                            ""carbonMonoxide"": {{
                                ""value"": ""tested""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = CarbonMonoxideDetector.CarbonMonoxideDetectorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = CarbonMonoxideState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""carbonMonoxideDetector"": {{
                            ""carbonMonoxide"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = CarbonMonoxideDetector.CarbonMonoxideDetectorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void ContactSensor_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new ContactSensor()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = ContactState.Closed
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""contactSensor"": {{
                            ""contact"": {{
                                ""value"": ""closed""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = ContactSensor.ContactSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = ContactState.Open;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""contactSensor"": {{
                            ""contact"": {{
                                ""value"": ""open""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = ContactSensor.ContactSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = ContactState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""contactSensor"": {{
                            ""contact"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = ContactSensor.ContactSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void DoorLock_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new DoorLock()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = LockState.Locked
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""lock"": {{
                            ""lock"": {{
                                ""value"": ""locked""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = DoorLock.LockFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = LockState.Unlocked;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
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

            status = JObject.Parse(statusJson);

            result = DoorLock.LockFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = LockState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""lock"": {{
                            ""lock"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = DoorLock.LockFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void LightSwitch_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new LightSwitch()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = SwitchState.Off
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""switch"": {{
                            ""switch"": {{
                                ""value"": ""off""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = LightSwitch.SwitchFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = SwitchState.On;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""switch"": {{
                            ""switch"": {{
                                ""value"": ""on""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = LightSwitch.SwitchFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = SwitchState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""switch"": {{
                            ""switch"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = LightSwitch.SwitchFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void MotionSensor_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new MotionSensor()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = MotionState.Active
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""motionSensor"": {{
                            ""motion"": {{
                                ""value"": ""active""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = MotionSensor.MotionSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = MotionState.InActive;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""motionSensor"": {{
                            ""motion"": {{
                                ""value"": ""inactive""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = MotionSensor.MotionSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = MotionState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""motionSensor"": {{
                            ""motion"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = MotionSensor.MotionSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void PresenceSensor_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new PresenceSensor()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = PresenceState.NotPresent
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""presenceSensor"": {{
                            ""presence"": {{
                                ""value"": ""not present""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = PresenceSensor.PresenceSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = PresenceState.Present;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""presenceSensor"": {{
                            ""presence"": {{
                                ""value"": ""present""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = PresenceSensor.PresenceSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = PresenceState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""presenceSensor"": {{
                            ""presence"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = PresenceSensor.PresenceSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }

        [Theory]
        [MemberData(nameof(ValidInstalledAppInstance))]
        public void WaterSensor_FromDynamic_ShouldReturnExpectedResult(InstalledAppInstance installedApp)
        {
            var expectedResult = new WaterSensor()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "MyDevice",
                CurrentState = WaterState.Dry
            };

            var deviceJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}""
            }}";

            var statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""waterSensor"": {{
                            ""water"": {{
                                ""value"": ""dry""
                            }}
                        }}
                    }}
                }}
            }}";

            dynamic device = JObject.Parse(deviceJson);
            dynamic status = JObject.Parse(statusJson);

            var result = WaterSensor.WaterSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = WaterState.Wet;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""waterSensor"": {{
                            ""water"": {{
                                ""value"": ""wet""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = WaterSensor.WaterSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            expectedResult.CurrentState = WaterState.Unknown;
            statusJson = $@"{{
                ""deviceId"": ""{expectedResult.Id}"",
                ""name"": ""NAME"",
                ""label"": ""{expectedResult.Label}"",
                ""locationId"": ""{installedApp.InstalledLocation.Id}"",
                ""components"" : {{
                    ""main"": {{
                        ""waterSensor"": {{
                            ""water"": {{
                                ""value"": ""unknown""
                            }}
                        }}
                    }}
                }}
            }}";

            status = JObject.Parse(statusJson);

            result = WaterSensor.WaterSensorFromDynamic(device,
                status);

            Assert.Equal(expectedResult, result);

            _ = expectedResult.GetHashCode();
            _ = expectedResult.ToJson();
        }
    }
}
