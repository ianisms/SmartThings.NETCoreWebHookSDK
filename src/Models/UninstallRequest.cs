using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class UninstallRequest : BaseRequest
    {
        [JsonProperty("uninstallData", Required = Required.Always)]
        public UninstallRequestData UninstallData { get; set; }

        public static UninstallRequest FromJson(string json) => JsonConvert.DeserializeObject<UninstallRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }
    public class UninstallRequestData
    {
        [JsonProperty("installedApp", Required = Required.Always)]
        public InstalledApp InstalledApp { get; set; }
    }

    public class UninstallResponseData
    {
    }

    public class UninstallResponse
    {
        [JsonProperty("uninstallData", Required = Required.Always)]
        public UninstallResponseData UninstallData { get; set; }
    }
}
