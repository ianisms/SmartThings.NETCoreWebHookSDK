using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models
{
    public partial class EventRequest : BaseRequest
    {
        [JsonProperty("eventData", Required = Required.Always)]
        public EventRequestData EventData { get; set; }
        public static EventRequest FromJson(string json) => JsonConvert.DeserializeObject<EventRequest>(json, ianisms.SmartThings.NETCoreWebHookSDK.Models.Converter.Settings);
    }

    public class DeviceEvent
    {
        [JsonProperty("subscriptionName", Required = Required.Always)]
        public string SubscriptionName { get; set; }

        [JsonProperty("eventId", Required = Required.Always)]
        public string EventId { get; set; }

        [JsonProperty("locationId", Required = Required.Always)]
        public string LocationId { get; set; }

        [JsonProperty("deviceId", Required = Required.Always)]
        public string DeviceId { get; set; }

        [JsonProperty("componentId", Required = Required.Always)]
        public string ComponentId { get; set; }

        [JsonProperty("capability", Required = Required.Always)]
        public string Capability { get; set; }

        [JsonProperty("attribute", Required = Required.Always)]
        public string Attribute { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty("stateChange", Required = Required.Always)]
        public bool StateChange { get; set; }
    }

    public class TimerEvent
    {
        [JsonProperty("eventId", Required = Required.Always)]
        public string EventId { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("time", Required = Required.Always)]
        public DateTime Time { get; set; }

        [JsonProperty("expression", Required = Required.Always)]
        public string Expression { get; set; }
    }

    public class RaisedEvent
    {
        [JsonProperty("eventType", Required = Required.Always)]
        public string EventType { get; set; }

        [JsonProperty("deviceEvent", Required = Required.Default)]
        public DeviceEvent DeviceEvent { get; set; }

        [JsonProperty("deviceEvent", Required = Required.Default)]
        public TimerEvent TimerEvent { get; set; }
    }

    public class EventRequestData
    {
        [JsonProperty("authToken", Required = Required.Always)]
        public string AuthToken { get; set; }

        [JsonProperty("installedApp", Required = Required.Always)]
        public InstalledApp InstalledApp { get; set; }

        [JsonProperty("events", Required = Required.Always)]
        public IEnumerable<RaisedEvent> Events { get; set; }
    }
}
