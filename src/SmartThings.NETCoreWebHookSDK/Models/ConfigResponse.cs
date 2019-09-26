using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class ConfigInitResponseData
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("permissions", Required = Required.Always)]
        public IEnumerable<string> Permissions { get; set; }

        [JsonProperty("firstPageId", Required = Required.Always)]
        public string FirstPageId { get; set; }
    }

    public class ConfigInitResponseConfigData
    {
        [JsonProperty("initialize", Required = Required.Always)]
        public ConfigInitResponseData InitData { get; set; }
    }

    public class ConfigSection
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("settings", Required = Required.Always)]
        public IEnumerable<ConfigSetting> Settings { get; set; }
    }

    public class ConfigPageResponseConfigData
    {
        [JsonProperty("page", Required = Required.Always)]
        public ConfigPage Page { get; set; }
    }

    public class ConfigInitResponse : ConfigResponse
    {
        [JsonProperty("configurationData", Required = Required.Default)]
        public ConfigInitResponseConfigData ConfigData { get; set; }
    }

    public class ConfigPageResponse : ConfigResponse
    {
        [JsonProperty("configurationData", Required = Required.Default)]
        public ConfigPageResponseConfigData ConfigData { get; set; }
    }

    public abstract class ConfigResponse : BaseResponse
    {
    }
}
