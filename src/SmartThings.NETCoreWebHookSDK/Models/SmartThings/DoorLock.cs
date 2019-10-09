using Newtonsoft.Json.Linq;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum LockState { Unlocked, Locked, Unknown };

    public class DoorLock : BaseModel
    {
        public LockState CurrentState { get; set; } = LockState.Unknown;

        public static LockState LockStateFromDynamic(dynamic status,
            bool isResponseStatus = false)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            if (isResponseStatus)
            {
                _ = status.components.main["lock"]["lock"].value ??
                    throw new ArgumentException("status.components.main.lock.lock.value is null!", nameof(status));
                status = status.components.main["lock"]["lock"].value;
            }

            var val = status.Value.ToLowerInvariant();

            var state = LockState.Unknown;
            if (!Enum.TryParse<LockState>(val, true, out state))
            {
                throw new ArgumentException($"DoorLock.LockStateFromDynamic status is an invalid value {status}", nameof(status));
            }
            else
            {
                return state;
            }
        }

        public static DoorLock LockFromDynamic(dynamic val,
            dynamic status = null)
        {
            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main["lock"]["lock"].value ??
                    throw new ArgumentException("status.components.main.lock.lock.value is null!", nameof(status));
                deviceStatus = status.components.main["lock"]["lock"].value;
            }

            return new DoorLock()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value,
                CurrentState = deviceStatus != null ? LockStateFromDynamic(deviceStatus) : LockState.Unknown
            };
        }

        public static dynamic GetDeviceCommand(bool value)
        {
            var cmd = value ? "lock" : "unlock";

            dynamic deviceCommands = JObject.Parse($@"{{
                ""commands"": [
		            {{
                        ""component"": ""main"",
                        ""capability"": ""lock"",
			            ""command"": ""{cmd}"",
			            ""arguments"": []
                    }}
                ]
            }}");

            return deviceCommands;
        }
    }
}
