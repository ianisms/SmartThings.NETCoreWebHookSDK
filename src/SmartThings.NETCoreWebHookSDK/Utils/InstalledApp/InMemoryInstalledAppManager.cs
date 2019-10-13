using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public class InMemoryInstalledAppManager : InstalledAppManager
    {
        public InMemoryInstalledAppManager(ILogger<IInstalledAppManager> logger,
            ISmartThingsAPIHelper smartThingsAPIHelper)
            : base (logger, smartThingsAPIHelper)
        {
        }

        public override async Task LoadCacheAsync()
        {
            InstalledAppCache = new Dictionary<string, Models.SmartThings.InstalledApp>();
        }

        public override async Task PersistCacheAsync()
        {
        }
    }
}
