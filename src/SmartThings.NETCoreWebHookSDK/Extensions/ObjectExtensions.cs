using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Newtonsoft.Json;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Common.JsonSerializerSettings);
        }
    }
}
