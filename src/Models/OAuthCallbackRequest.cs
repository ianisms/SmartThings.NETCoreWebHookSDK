using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class OAuthCallbackRequest : BaseRequest
    {
        public static OAuthCallbackRequest FromJson(string json) => JsonConvert.DeserializeObject<OAuthCallbackRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }

    public class OAuthCallbackResponseData
    {
    }

    public class OAuthCallbackResponse
    {
        [JsonProperty("oAuthCallbackData", Required = Required.Always)]
        public OAuthCallbackResponseData OauthData { get; set; }
    }

}
