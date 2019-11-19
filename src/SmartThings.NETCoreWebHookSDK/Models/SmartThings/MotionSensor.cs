#region Copyright
// <copyright file="MotionSensor.cs" company="Ian N. Bennett">
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
    public enum MotionState { InActive, Active, Unknown };

    public class MotionSensor : BaseModel
    {
        public MotionState CurrentState { get; set; } = MotionState.Unknown;

        public static MotionState MotionStateFromDynamic(dynamic status,
            bool isResponseStatus = false)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            if (isResponseStatus)
            {
                _ = status.components.main.motionSensor.motion.value ??
                    throw new ArgumentException("status.components.main.motionSensor.motion.value is null!",
                    nameof(status));
                status = status.components.main.motionSensor.motion.value;
            }

            var val = status.Value.ToLowerInvariant();

            var state = MotionState.Unknown;
            if (!Enum.TryParse<MotionState>(val, true, out state))
            {
                throw new ArgumentException($"MotionStateFromDynamic status is an invalid value {status}",
                    nameof(status));
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
                    throw new ArgumentException("status.components.main.motionSensor.motion.value is null!",
                    nameof(status));
                deviceStatus = status.components.main.motionSensor.motion.value;
            }

            return new MotionSensor()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value,
                CurrentState = deviceStatus != null ? MotionStateFromDynamic(deviceStatus) : MotionState.Unknown
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MotionSensor))
            {
                return false;
            }

            var targetObj = (obj as MotionSensor);

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
