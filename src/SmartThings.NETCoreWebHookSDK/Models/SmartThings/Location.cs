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

using CoordinateSharp;
using System;
using TimeZoneConverter;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum TemperatureScale { F, C, Unknown }

    public class Location
    {
        public string Id { get; set; }
        public string Label { get; set; }
        private Celestial celestial;
        public string CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RegionRadius { get; set; }
        public TemperatureScale TempScale { get; set; } = TemperatureScale.Unknown;
        public string TimeZoneId { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
        public string Locale { get; set; }

        private void InitTimeZone(DateTime? dtOverride = null)
        {
            if (TimeZone == null ||
                celestial == null)
            {
                TimeZone = TZConvert.GetTimeZoneInfo(TimeZoneId);

                var dt = GetDateTimeInLocation(dtOverride);
                celestial = Celestial.CalculateCelestialTimes(Latitude, Longitude, dt, TimeZone.GetUtcOffset(dt).TotalHours);
            }
        }

        public DateTime GetDateTimeInLocation(DateTime? dtOverride = null)
        {
            DateTime dt = dtOverride ?? DateTime.Now;
            return TimeZoneInfo.ConvertTime(dt, TimeZone);
        }


        public DateTime GetSunrise(DateTime? dtOverride = null)
        {
            InitTimeZone(dtOverride);
            return celestial.SunRise.Value;
            //return TimeZoneInfo.ConvertTimeFromUtc(celestial.SunRise.Value, TimeZone);
        }

        public DateTime GetSunset(DateTime? dtOverride = null)
        {
            InitTimeZone(dtOverride);
            return celestial.SunSet.Value;
            //return TimeZoneInfo.ConvertTimeFromUtc(celestial.SunSet.Value, TimeZone);
        }

        public bool IsAfterSunrise()
        {
            InitTimeZone();
            return (GetDateTimeInLocation() > GetSunrise());
        }

        public bool IsAfterSunset()
        {
            InitTimeZone();
            return (GetDateTimeInLocation() > GetSunset());
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
