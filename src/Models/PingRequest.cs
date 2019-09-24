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

    public class PingData
    {
        [JsonProperty("challenge", Required = Required.Always)]
        public Guid Challenge { get; set; }
    }

    public class PingResponse
    {
        [JsonProperty("pingData", Required = Required.Always)]
        public PingData PingData { get; set; }
        public static PingResponse FromPingRequest(PingRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return new PingResponse()
            {
                PingData = request.PingData
            };
        }
    }
}
