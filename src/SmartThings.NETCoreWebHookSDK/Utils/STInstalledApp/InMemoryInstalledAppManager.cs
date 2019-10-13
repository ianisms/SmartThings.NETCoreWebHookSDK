using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.STInstalledApp
{
    public class InMemoryInstalledAppManager : InstalledAppManager
    {
        private Dictionary<string, InstalledApp> installedAppCache;

        public InMemoryInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
            : base (logger, smartThingsAPIHelper)
        {
        }

        public override async Task LoadCacheAsync()
        {
            installedAppCache = new Dictionary<string, InstalledApp>();
        }

        public override async Task PersistCacheAsync()
        {
        }
    }
}
