using Newtonsoft.Json;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class PingData
    {
        [JsonProperty("challenge", Required = Required.Always)]
        public Guid Challenge { get; set; }
    }
}
