using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class InstallRequest : BaseRequest
    {
        [JsonProperty("installData", Required = Required.Always)]
        public InstallData InstallData { get; set; }

        [JsonProperty("settings", Required = Required.Default)]
        public ConfigSetting Settings { get; set; }

        public static InstallRequest FromJson(string json) => JsonConvert.DeserializeObject<InstallRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }

    public class InstallData
    {
        [JsonProperty("authToken", Required = Required.Always)]
        public string AuthToken { get; set; }

        [JsonProperty("refreshToken", Required = Required.Always)]
        public string RefreshToken { get; set; }

        [JsonProperty("installedApp", Required = Required.Always)]
        public InstalledApp InstalledApp { get; set; }
    }

    public class InstalledApp
    {
        [JsonProperty("installedAppId", Required = Required.Always)]
        public Guid InstalledAppId { get; set; }

        [JsonProperty("locationId", Required = Required.Always)]
        public Guid LocationId { get; set; }

        [JsonProperty("permissions", Required = Required.Always)]
        public IEnumerable<string> Permissions { get; set; }
    }

    public class InstallResponseData
    {
    }

    public class InstallResponse
    {
        [JsonProperty("installData", Required = Required.Always)]
        public InstallResponseData InstallData { get; set; }
    }
}
