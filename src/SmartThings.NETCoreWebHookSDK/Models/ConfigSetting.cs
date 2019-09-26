using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
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
}
