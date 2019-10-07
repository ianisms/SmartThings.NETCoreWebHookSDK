using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum PresenceState { NotPresent, Present, Unknown };

    public class PresenceSensor : BaseModel
    {
        public PresenceState CurrentState { get; set; } = PresenceState.Unknown;
        public string FriendlyName { get; set; }

        public static PresenceState PresenceStateFromDynamic(dynamic status)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            var val = status.Value.ToLowerInvariant().Replace(" ", "");

            var state = PresenceState.Unknown;
            if(!Enum.TryParse<PresenceState>(val, true, out state))
            {
                throw new ArgumentException($"PresenceSensor.PresenceStateFromDynamic status is an invalid value {status}", nameof(status));
            }
            else
            {
                return state;
            }
        }

        public static PresenceSensor PresenceSensorFromDynamic(dynamic val,
            dynamic status = null,
            dynamic presenceSensorNamePattern = null)
        {
            var label = val.label.Value;
            var friendlyName = label;

            if (presenceSensorNamePattern != null &&
                presenceSensorNamePattern[0] != null &&
                presenceSensorNamePattern[0].stringConfig != null &&
                presenceSensorNamePattern[0].stringConfig.value != null &&
                presenceSensorNamePattern[0].stringConfig.Value != null)
            {
                var presenceSensorNamePatternValue = presenceSensorNamePattern[0].stringConfig.Value;
                var psnpIndex = friendlyName.IndexOf(presenceSensorNamePatternValue, StringComparison.Ordinal);
                if (psnpIndex > 0)
                {
                    friendlyName = friendlyName.Substring(0, psnpIndex);
                }
            }

            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main.presenceSensor.presence.value ??
                    throw new ArgumentException("status.components.main.presenceSensor.presence.value is null!", nameof(status));
                deviceStatus = status.components.main.presenceSensor.presence.value;
            }

            return new PresenceSensor()
            {
                Id = val.deviceId.Value,
                Label = label,
                CurrentState = deviceStatus != null ? PresenceStateFromDynamic(deviceStatus) : PresenceState.Unknown,
                FriendlyName = friendlyName
            };
        }
    }
}
