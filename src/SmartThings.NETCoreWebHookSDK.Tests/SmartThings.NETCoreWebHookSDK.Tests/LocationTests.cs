#region Copyright
// <copyright file="LocationTests.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class LocationTests
    {
        [Theory]
        [InlineData(@"{
            ""locationId"": ""6b3d1909-1e1c-43ec-adc2-5f941de4fbf9"",
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
            ""locationId"": ""6b3d1909-1e1c-43ec-adc2-5f941de4fbf9"",
            ""name"": ""Home"",
            ""countryCode"": ""USA"",
            ""latitude"": ""51.5074"",
            ""longitude"": ""0.1278"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""C"",
            ""timeZoneId"": ""Europe/London"",
            ""locale"": ""en""
        }")]
        public void LocationShouldParseFromDynamic(string locJson)
        {
            dynamic loc = JObject.Parse(locJson);
            var location = Location.LocationFromDynamic(loc);
            Assert.NotNull(location);
        }

        [Theory]
        [InlineData(@"{
            ""locationId"": ""6b3d1909-1e1c-43ec-adc2-5f941de4fbf9"",
            ""name"": ""Home"",
            ""countryCode"": ""USA"",
            ""latitude"": ""40.347054"",
            ""longitude"": ""-74.064308"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""F"",
            ""timeZoneId"": ""America/New_York"",
            ""locale"": ""en""
        }",
        "12/18/2019",
        "12/18/2019 07:14:00",
        "12/18/2019 16:32:00")]
        [InlineData(@"{
            ""locationId"": ""6b3d1909-1e1c-43ec-adc2-5f941de4fbf9"",
            ""name"": ""Home"",
            ""countryCode"": ""UK"",
            ""latitude"": ""51.5074"",
            ""longitude"": ""0.1278"",
            ""regionRadius"": ""150"",
            ""temperatureScale"": ""C"",
            ""timeZoneId"": ""Europe/London"",
            ""locale"": ""en""
        }",
        "12/18/2019",
        "12/18/2019 08:02:00",
        "12/18/2019 15:52:00")]
        public void LocationSunriseSunsetShouldBeWithinTwoMinutesOfExpected(string locJson,
            string dateVal,
            string expectedSunriseVal,
            string expectedSunsetVal)
        {
            dynamic loc = JObject.Parse(locJson);
            var location = Location.LocationFromDynamic(loc);
            var dt = DateTime.Parse(dateVal);
            var expectedSunrise = DateTime.Parse(expectedSunriseVal);
            var expectedSunset = DateTime.Parse(expectedSunsetVal);

            var sunrise = location.GetSunrise(dt);
            var sunset = location.GetSunset(dt);

            var sunriseDiff = (sunrise - expectedSunrise).Duration();
            var sunsetDiff = (sunset - expectedSunset).Duration();

            // within 2 minutes
            Assert.True(sunriseDiff.TotalMinutes < 2);
            Assert.True(sunsetDiff.TotalMinutes < 2);
        }
    }
}
