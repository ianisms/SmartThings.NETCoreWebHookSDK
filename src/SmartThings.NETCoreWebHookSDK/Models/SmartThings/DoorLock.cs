#region Copyright
// <copyright file="DoorLock.cs" company="Ian N. Bennett">
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

            var json = $@"{{
                ""commands"": [
		            {{
                        ""component"": ""main"",
                        ""capability"": ""lock"",
			            ""command"": ""{cmd}"",
			            ""arguments"": []
                    }}
                ]
            }}";

            dynamic deviceCommands = JObject.Parse(json);

            return deviceCommands;
        }
    }
}
