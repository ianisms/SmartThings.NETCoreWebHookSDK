using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyWebhookLib.Models
{
    public class MyState
    {
        public string InstalledAppId { get; set; }
        public bool IsAppEnabled { get; set; }
        public IList<LightSwitch> LightSwitches { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
