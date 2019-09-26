using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestLifecycle
    {
        [EnumMember(Value = "PING")]
        Ping,
        [EnumMember(Value = "CONFIGURATION")]
        Configuration,
        [EnumMember(Value = "INSTALL")]
        Install,
        [EnumMember(Value = "UPDATE")]
        Update,
        [EnumMember(Value = "EVENT")]
        Event,
        [EnumMember(Value = "UNINSTALL")]
        Uninstall,
        [EnumMember(Value = "OAUTH_CALLBACK")]
        OAuthCallback
    }

    public abstract class BaseRequest
    {
        [JsonProperty("lifecycle", Required = Required.Always)]
        public RequestLifecycle Lifecycle { get; set; }

        [JsonProperty("executionId", Required = Required.Always)]
        public Guid ExecutionId { get; set; }

        [JsonProperty("locale", Required = Required.Always)]
        public string Locale { get; set; }

        [JsonProperty("version", Required = Required.Always)]
        public string Version { get; set; }
    }

    public class DeviceConfig
    {
        [JsonProperty("deviceId", Required = Required.Always)]
        public Guid DeviceId { get; set; }

        [JsonProperty("componentId", Required = Required.Always)]
        public string ComponentId { get; set; }
    }
}
