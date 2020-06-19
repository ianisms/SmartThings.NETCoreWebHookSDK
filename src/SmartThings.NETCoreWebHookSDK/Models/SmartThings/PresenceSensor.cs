#region Copyright
// <copyright file="PresenceSensor.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
