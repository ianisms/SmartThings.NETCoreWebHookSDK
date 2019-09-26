using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class InstalledApp
    {
        [JsonProperty("installedAppId", Required = Required.Always)]
        public Guid InstalledAppId { get; set; }

        [JsonProperty("locationId", Required = Required.Always)]
        public Guid LocationId { get; set; }

        [JsonProperty("permissions", Required = Required.Always)]
        public IEnumerable<string> Permissions { get; set; }
    }
}
