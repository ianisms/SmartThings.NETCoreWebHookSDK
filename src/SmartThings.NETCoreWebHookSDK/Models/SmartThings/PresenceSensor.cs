#region Copyright
// <copyright file="PresenceSensor.cs" company="Ian N. Bennett">
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
    public enum PresenceState { NotPresent, Present, Unknown };

    public class PresenceSensor : BaseModel
    {
        public PresenceState CurrentState { get; set; } = PresenceState.Unknown;
        public string FriendlyName { get; set; }

        public static PresenceState PresenceStateFromDynamic(dynamic status,
            bool isResponseStatus = false)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            if (isResponseStatus)
            {
                _ = status.components.main.presenceSensor.presence.value ??
                    throw new ArgumentException("status.components.main.presenceSensor.presence.value is null!",
                    nameof(status));
                status = status.components.main.presenceSensor.presence.value;
            }

            var val = status.Value.ToLowerInvariant().Replace(" ", "");

            var state = PresenceState.Unknown;
            if (!Enum.TryParse<PresenceState>(val, true, out state))
            {
                throw new ArgumentException($"PresenceSensor.PresenceStateFromDynamic status is an invalid value {status}",
                    nameof(status));
            }
            else
            {
                return state;
            }
        }

        public static PresenceSensor PresenceSensorFromDynamic(dynamic val,
            dynamic status = null,
            string presenceSensorNamePattern = null)
        {
            var label = val.label.Value;
            var friendlyName = label;

            if (!string.IsNullOrWhiteSpace(presenceSensorNamePattern))
            {
                var psnpIndex = friendlyName.IndexOf(presenceSensorNamePattern, StringComparison.Ordinal);
                if (psnpIndex > 0)
                {
                    friendlyName = friendlyName.Substring(0, psnpIndex);
                }
            }

            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main.presenceSensor.presence.value ??
                    throw new ArgumentException("status.components.main.presenceSensor.presence.value is null!",
                    nameof(status));
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

        public override bool Equals(object obj)
        {
            if (!(obj is PresenceSensor))
            {
                return false;
            }

            var targetObj = (obj as PresenceSensor);

            return base.Equals(obj) &&
                this.CurrentState.Equals(targetObj.CurrentState);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() +
                this.CurrentState.GetHashCode();
        }
    }
}
