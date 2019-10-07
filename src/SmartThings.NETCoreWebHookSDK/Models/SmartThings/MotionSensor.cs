using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum MotionState { InActive, Active, Unknown };

    public class MotionSensor : BaseModel
    {
        public MotionState CurrentState { get; set; } = MotionState.Unknown;

        public static MotionState MotionStateFromDynamic(dynamic status)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            var val = status.Value.ToLowerInvariant();

            var state = MotionState.Unknown;
            if (!Enum.TryParse<MotionState>(val, true, out state))
            {
                throw new ArgumentException($"MotionStateFromDynamic status is an invalid value {status}", nameof(status));
            }
            else
            {
                return state;
            }
        }

        public static MotionSensor MotionSensorFromDynamic(dynamic val,
            dynamic status = null)
        {
            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main.motionSensor.motion.value ??
                    throw new ArgumentException("status.components.main.motionSensor.motion.value is null!", nameof(status));
                deviceStatus = status.components.main.motionSensor.motion.value;
            }

            return new MotionSensor()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value,
                CurrentState = deviceStatus != null ? MotionStateFromDynamic(deviceStatus) : MotionState.Unknown
            };
        }
    }
}
