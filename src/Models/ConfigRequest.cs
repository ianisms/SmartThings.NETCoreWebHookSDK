
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

    public class ConfigSetting
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SettingsType
        {
            [EnumMember(Value = "DEVICE")]
            Device,
            [EnumMember(Value = "TEXT")]
            Text,
            [EnumMember(Value = "BOOLEAN")]
            Boolean,
            [EnumMember(Value = "ENUM")]
            Enum,
            [EnumMember(Value = "LINK")]
            Link,
            [EnumMember(Value = "PAGE")]
            Page,
            [EnumMember(Value = "IMAGE")]
            Image,
            [EnumMember(Value = "ICON")]
            Icon,
            [EnumMember(Value = "TIME")]
            Time,
            [EnumMember(Value = "PARAGRAPH")]
            Paragraph,
            [EnumMember(Value = "EMAIL")]
            Email,
            [EnumMember(Value = "DECIMAL")]
            DecimalType,
            [EnumMember(Value = "NUMBER")]
            Number,
            [EnumMember(Value = "PHONE")]
            Phone,
            [EnumMember(Value = "OAUTH")]
            Oauth
        }

        [JsonProperty("id", Required = Required.Default)]
        public string Id { get; set; }

        [JsonProperty("name", Required = Required.Default)]
        public string Name { get; set; }

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty("type", Required = Required.Default)]
        public SettingsType Type { get; set; }

        [JsonProperty("required", Required = Required.Default)]
        public bool IsRequired { get; set; }

        [JsonProperty("multiple", Required = Required.Default)]
        public bool IsMultiple { get; set; }

        [JsonProperty("capabilities", Required = Required.Default)]
        public IEnumerable<string> Capabilities { get; set; }

        [JsonProperty("permissions", Required = Required.Default)]
        public IEnumerable<string> Permissions { get; set; }

        [JsonProperty("defaultValue", Required = Required.Default)]
        public string DefaultValue { get; set; }

        [JsonProperty("image", Required = Required.Default)]
        public Uri Image { get; set; }

        [JsonProperty("Width", Required = Required.Default)]
        public int Width { get; set; }

        [JsonProperty("Height", Required = Required.Default)]
        public int Height { get; set; }

        [JsonProperty("page", Required = Required.Default)]
        public string Page { get; set; }

        [JsonProperty("urlTemplate", Required = Required.Default)]
        public Uri UrlTemplate { get; set; }
    }

    public class ConfigPage
    {
        [JsonProperty("pageId", Required = Required.Default)]
        public string PageId { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("previousPageId", Required = Required.Default)]
        public string PreviousPageId { get; set; }

        [JsonProperty("nextPageId", Required = Required.Default)]
        public string NextPageId { get; set; }

        [JsonProperty("complete", Required = Required.Always)]
        public bool IsComplete { get; set; }

        [JsonProperty("sections", Required = Required.Default)]
        public IEnumerable<ConfigSection> Sections { get; set; }

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

    public abstract class ConfigResponse
    {
    }
}
