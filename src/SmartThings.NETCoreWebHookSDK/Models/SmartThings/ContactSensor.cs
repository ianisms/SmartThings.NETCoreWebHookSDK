using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public enum ContactState { Closed, Open, Unknown };

    public class ContactSensor : BaseModel
    {
        public ContactState CurrentState { get; set; } = ContactState.Unknown;

        public static ContactState ContactStateFromDynamic(dynamic status,
            bool isResponseStatus = false)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            if (isResponseStatus)
            {
                _ = status.components.main.contactSensor.contact.value ??
                    throw new ArgumentException("status.components.main.contactSensor.contact.value is null!", nameof(status));
                status = status.components.main.contactSensor.contact.value;
            }

            var val = status.Value.ToLowerInvariant();

            var state = ContactState.Unknown;
            if (!Enum.TryParse<ContactState>(val, true, out state))
            {
                throw new ArgumentException($"ContactSensor.MotionStateFromDynamic status is an invalid value {status}", nameof(status));
            }
            else
            {
                return state;
            }
        }

        public static ContactSensor ContactSensorFromDynamic(dynamic val,
            dynamic status = null)
        {
            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main.contactSensor.contact.value ??
                    throw new ArgumentException("status.components.main.contactSensor.contact.value is null!", nameof(status));
                deviceStatus = status.components.main.contactSensor.contact.value;
            }

            return new ContactSensor()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value,
                CurrentState = deviceStatus != null ? ContactStateFromDynamic(deviceStatus) : ContactState.Unknown
            };
        }
    }
}
