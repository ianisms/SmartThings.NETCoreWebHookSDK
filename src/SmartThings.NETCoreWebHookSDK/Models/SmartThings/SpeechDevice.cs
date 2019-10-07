using Newtonsoft.Json.Linq;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public class SpeechDevice : BaseModel
    {
        public static SpeechDevice SpeechDeviceFromDynamic(dynamic val)
        {
            return new SpeechDevice()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value
            };
        }

        public static dynamic GetSpeakDeviceCommand(string message)
        {
            dynamic deviceCommands = JObject.Parse($@"{{
                ""commands"": [
		            {{
                        ""component"": ""main"",
                        ""capability"": ""speechSynthesis"",
			            ""command"": ""speak"",
			            ""arguments"": [""{message}""]
                    }}
                ]
            }}");

            return deviceCommands;
        }
    }
}
