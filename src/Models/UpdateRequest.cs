using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class UpdateRequest : BaseRequest
    {
        [JsonProperty("updateData", Required = Required.Always)]
        public UpdateRequestData UpdateData { get; set; }

        public static UpdateRequest FromJson(string json) => JsonConvert.DeserializeObject<UpdateRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }
    public class UpdateRequestData
    {
        [JsonProperty("authToken", Required = Required.Always)]
        public string AuthToken { get; set; }

        [JsonProperty("refreshToken", Required = Required.Always)]
        public string RefreshToken { get; set; }

        [JsonProperty("installedApp", Required = Required.Always)]
        public InstalledApp InstalledApp { get; set; }
    }

    public class UpdateResponseData
    {
    }

    public class UpdateResponse
    {
        [JsonProperty("updateData", Required = Required.Always)]
        public UpdateResponseData UpdateData { get; set; }
    }
}
