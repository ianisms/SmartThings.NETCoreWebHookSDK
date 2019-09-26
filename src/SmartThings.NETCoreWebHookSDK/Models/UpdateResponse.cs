using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class UpdateResponseData
    {
    }

    public class UpdateResponse : BaseResponse
    {
        [JsonProperty("updateData", Required = Required.Always)]
        public UpdateResponseData UpdateData { get; set; }
    }
}
