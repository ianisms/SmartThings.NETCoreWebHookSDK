using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
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
            Assert.Equal(CommonUtils.GetValidInstalledAppInstance().InstalledAppId, value);
        }
    }

    public class CommonUtils
    {
        private static Dictionary<string, InstalledAppInstance> iaCache;
        public static Dictionary<string, InstalledAppInstance> GetIACache()
        {
            if (iaCache == null)
            {
                var ia = GetValidInstalledAppInstance();
                iaCache = new Dictionary<string, InstalledAppInstance>()
                {
                    { ia.InstalledAppId, ia }
                };
            }

            return iaCache;
        }

        private static Dictionary<string, object> stateCache;
        public static Dictionary<string, object> GetStateCache()
        {
            if (stateCache == null)
            {
                var ia = GetValidInstalledAppInstance();
                stateCache = new Dictionary<string, object>()
                {
                    { ia.InstalledAppId, GetStateObject() }
                };
            }

            return stateCache;
        }

        private static InstalledAppInstance installedAppInstance;
        public static InstalledAppInstance GetValidInstalledAppInstance()
        {
            if (installedAppInstance == null)
            {
                installedAppInstance = new InstalledAppInstance()
                {
                    InstalledAppId = "d692699d-e7a6-400d-a0b7-d5be96e7a564",
                    InstalledLocation = new Location()
                    {
                        CountryCode = "US",
                        Id = "e675a3d9-2499-406c-86dc-8a492a88649",
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
    }
}
