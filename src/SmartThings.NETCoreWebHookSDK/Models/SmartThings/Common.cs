using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum Lifecycle { Ping, Configuration, Install, Update, Event, Uninstall, Oauthcallback, Unknown }

    public static class Common
    {
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            }
        }
    }
}