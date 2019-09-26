
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class ConfigRequest : BaseRequest
    {
        [JsonProperty("configurationData", Required = Required.Always)]
        public ConfigRequestData ConfigurationData { get; set; }

        [JsonProperty("settings", Required = Required.Default)]
        public ConfigSetting Settings { get; set; }
        public static ConfigRequest FromJson(string json) => JsonConvert.DeserializeObject<ConfigRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }

    public class ConfigRequestData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum RequestPhase
        {
            [EnumMember(Value = "INITIALIZE")]
            Initialize,
            [EnumMember(Value = "PAGE")]
            Page
        }

        [JsonProperty("installedAppId", Required = Required.Default)]
        public string InstalledAppId { get; set; }

        [JsonProperty("phase", Required = Required.Always)]
        public RequestPhase Phase { get; set; }

        [JsonProperty("page", Required = Required.Default)]
        public ConfigPage Page { get; set; }
    }
}
