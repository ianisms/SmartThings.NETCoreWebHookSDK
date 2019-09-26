using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class InstallResponseData
    {
    }

    public class InstallResponse : BaseResponse
    {
        [JsonProperty("installData", Required = Required.Always)]
        public InstallResponseData InstallData { get; set; }
    }
}
