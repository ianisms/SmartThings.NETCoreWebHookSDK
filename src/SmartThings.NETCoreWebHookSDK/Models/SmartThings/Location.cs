#region Copyright
// <copyright file="Location.cs" company="Ian N. Bennett">
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

using Innovative.SolarCalculator;
using System;
using TimeZoneConverter;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum TemperatureScale { F, C, Unknown }

    public class Location
    {
        public string Id { get; set; }
        public string Label { get; set; }
        private SolarTimes solarTimes;
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RegionRadius { get; set; }
        public TemperatureScale TempScale { get; set; } = TemperatureScale.Unknown;
        public string TimeZoneId { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
        public string Locale { get; set; }

        private void InitTimeZone()
        {
            if (TimeZone == null ||
                solarTimes == null)
            {
                TimeZone = TZConvert.GetTimeZoneInfo(TimeZoneId);
                solarTimes = new SolarTimes(GetCurrentTime(), Latitude, Longitude);
            }
        }

        public DateTime GetCurrentTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZone);
        }


        public DateTime GetSunrise()
        {
            InitTimeZone();
            return TimeZoneInfo.ConvertTimeFromUtc(solarTimes.Sunrise.ToUniversalTime(), TimeZone);
        }

        public DateTime GetSunset()
        {
            InitTimeZone();
            return TimeZoneInfo.ConvertTimeFromUtc(solarTimes.Sunset.ToUniversalTime(), TimeZone);
        }

        public bool IsAfterSunrise()
        {
            InitTimeZone();
            return (GetCurrentTime() > GetSunrise());
        }

        public bool IsAfterSunset()
        {
            InitTimeZone();
            return (GetCurrentTime() > GetSunset());
        }

        public static TemperatureScale TemperatureScaleFromDynamic(dynamic val)
        {
            var temperatureScaleVal = val.Value.ToLowerInvariant();
            if (temperatureScaleVal == "f")
            {
                return TemperatureScale.F;
            }
            else if (temperatureScaleVal == "c")
            {
                return TemperatureScale.C;
            }
            else
            {
                return TemperatureScale.Unknown;
            }
        }

        public static Location LocationFromDynamic(dynamic val)
        {
            return new Location()
            {
                Id = val.locationId.Value,
                Label = val.name.Value,
                Latitude = double.Parse(val.latitude.Value),
                Longitude = double.Parse(val.longitude.Value),
                RegionRadius = int.Parse(val.regionRadius.Value),
                TempScale = TemperatureScaleFromDynamic(val.temperatureScale),
                TimeZoneId = val.timeZoneId.Value,
                Locale = val.locale.Value
            };
        }
    }
}
