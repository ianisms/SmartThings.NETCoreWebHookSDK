#region Copyright
// <copyright file="Location.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
#endregion
using CoordinateSharp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        }

        public DateTime GetSunset(DateTime? dtOverride = null)
        {
            InitTimeZone(dtOverride);
            return celestial.SunSet.Value;
        }

        public bool IsAfterSunrise(ILogger logger = null)
        {
            InitTimeZone();
            var dtil = GetDateTimeInLocation();
            var sunrise = GetSunrise();
            var isAfterSunrise = (dtil > sunrise);

            if(logger != null) {
                logger.LogDebug($"IsAfterSunrise {{ dtil: {dtil}, sunrise: {sunrise}, isAfterSunrise: {isAfterSunrise} }}");
            }

            return isAfterSunrise;
        }

        public bool IsAfterSunset(ILogger logger = null)
        {
            InitTimeZone();
            var dtil = GetDateTimeInLocation();
            var sunset = GetSunrise();
            var isAfterSunset = (dtil > sunset);

            if (logger != null)
            {
                logger.LogDebug($"IsAfterSunset {{ dtil: {dtil}, sunset: {sunset}, isAfterSunrise: {isAfterSunset} }}");
            }

            return isAfterSunset;
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
            var loc = new Location()
            {
                Id = val.locationId,
                Label = val.name,
                Latitude = val.latitude,
                Longitude = val.longitude,
                RegionRadius = (int)val.regionRadius,
                TempScale = TemperatureScaleFromDynamic(val.temperatureScale),
                TimeZoneId = val.timeZoneId,
                CountryCode = val.countryCode,
                Locale = val.locale
            };

            return loc;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Location))
            {
                return false;
            }

            var targetLoc = (obj as Location);

            return this.Id.Equals(targetLoc.Id, StringComparison.Ordinal) &&
                this.CountryCode.Equals(targetLoc.CountryCode, StringComparison.Ordinal) &&
                this.Label.Equals(targetLoc.Label, StringComparison.Ordinal) &&
                this.Latitude.Equals(targetLoc.Latitude) &&
                this.Locale.Equals(targetLoc.Locale, StringComparison.Ordinal) &&
                this.Longitude.Equals(targetLoc.Longitude) &&
                this.RegionRadius.Equals(targetLoc.RegionRadius) &&
                this.TempScale.Equals(targetLoc.TempScale) &&
                this.TimeZoneId.Equals(targetLoc.TimeZoneId, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode(StringComparison.Ordinal) +
                this.CountryCode.GetHashCode(StringComparison.Ordinal) +
                this.Label.GetHashCode(StringComparison.Ordinal) +
                this.Latitude.GetHashCode() +
                this.Locale.GetHashCode(StringComparison.Ordinal) +
                this.Longitude.GetHashCode() +
                this.RegionRadius.GetHashCode() +
                this.TempScale.GetHashCode() +
                this.TimeZoneId.GetHashCode(StringComparison.Ordinal);
        }
    }
}
