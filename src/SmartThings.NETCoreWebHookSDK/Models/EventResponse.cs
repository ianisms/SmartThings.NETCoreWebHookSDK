using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{

    public class EventResponseData
    {
    }

    public class EventResponse : BaseResponse
    {
        [JsonProperty("eventData", Required = Required.Always)]
        public EventResponseData EventData { get; set; }
    }
}
