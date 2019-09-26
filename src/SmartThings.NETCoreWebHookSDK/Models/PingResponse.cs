using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class PingResponse : BaseResponse
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
