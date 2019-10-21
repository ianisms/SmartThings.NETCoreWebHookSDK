#region Copyright
// <copyright file="ContactSensor.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// SmartThings.NETCoreWebHookSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SmartThings.NETCoreWebHookSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
#endregion

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
