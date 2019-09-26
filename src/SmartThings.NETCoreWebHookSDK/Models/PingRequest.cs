using Newtonsoft.Json;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public partial class PingRequest : BaseRequest
    {
        [JsonProperty("pingData", Required = Required.Always)]
        public PingData PingData { get; set; }
        public static PingRequest FromJson(string json) => JsonConvert.DeserializeObject<PingRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }
}
