using Newtonsoft.Json;
using System.Collections.Generic;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public class ConfigPage
    {
        [JsonProperty("pageId", Required = Required.Default)]
        public string PageId { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("previousPageId", Required = Required.Default)]
        public string PreviousPageId { get; set; }

        [JsonProperty("nextPageId", Required = Required.Default)]
        public string NextPageId { get; set; }

        [JsonProperty("complete", Required = Required.Always)]
        public bool IsComplete { get; set; }

        [JsonProperty("sections", Required = Required.Default)]
        public IEnumerable<ConfigSection> Sections { get; set; }

    }
}
