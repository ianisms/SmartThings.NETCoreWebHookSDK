using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class UninstallResponseData
    {
    }

    public class UninstallResponse : BaseResponse
    {
        [JsonProperty("uninstallData", Required = Required.Always)]
        public UninstallResponseData UninstallData { get; set; }
    }
}
