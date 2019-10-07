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
            if(TimeZone == null)
            {
                TimeZone = TZConvert.GetTimeZoneInfo(TimeZoneId);
                solarTimes = new SolarTimes(GetCurrentTime(), Latitude, Longitude);
            }
        }

        public DateTime GetCurrentTime()
        {
            if(TimeZone == null)
            {
                InitTimeZone();
            }
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZone);
        }


        public DateTime GetSunrise()
        {
            if (TimeZone == null)
            {
                InitTimeZone();
            }
            return TimeZoneInfo.ConvertTimeFromUtc(solarTimes.Sunrise.ToUniversalTime(), TimeZone);
        }

        public DateTime GetSunset()
        {
            if (TimeZone == null)
            {
                InitTimeZone();
            }
            return TimeZoneInfo.ConvertTimeFromUtc(solarTimes.Sunset.ToUniversalTime(), TimeZone);
        }

        public bool IsAfterSunrise()
        {
            if (TimeZone == null)
            {
                InitTimeZone();
            }
            return (GetCurrentTime() > GetSunrise());
        }

        public bool IsAfterSunset()
        {
            if (TimeZone == null)
            {
                InitTimeZone();
            }
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
