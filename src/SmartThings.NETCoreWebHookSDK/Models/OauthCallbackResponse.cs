using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class OAuthCallbackResponseData
    {
    }

    public class OAuthCallbackResponse : BaseResponse
    {
        [JsonProperty("oAuthCallbackData", Required = Required.Always)]
        public OAuthCallbackResponseData OauthData { get; set; }
    }
}
