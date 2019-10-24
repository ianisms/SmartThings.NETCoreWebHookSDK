#region Copyright
// <copyright file="LightSwitch.cs" company="Ian N. Bennett">
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
    public enum SwitchState { Off, On, Unknown };

    public class LightSwitch : BaseModel
    {
        public SwitchState CurrentState { get; set; } = SwitchState.Unknown;

        public static SwitchState SwitchStateFromDynamic(dynamic status,
            bool isResponseStatus = false)
        {
            _ = status ?? throw new ArgumentNullException(nameof(status));

            if (isResponseStatus)
            {
                _ = status.components.main["switch"]["switch"].value ??
                    throw new ArgumentException("status.components.main.switch.switch.value is null!",
                    nameof(status));
                status = status.components.main["switch"]["switch"].value;
            }

            var val = status.Value.ToLowerInvariant();

            var state = SwitchState.Unknown;
            if (!Enum.TryParse<SwitchState>(val, true, out state))
            {
                throw new ArgumentException($"Switch.SwitchStateFromDynamic status is an invalid value {status}",
                    nameof(status));
            }
            else
            {
                return state;
            }
        }

        public static LightSwitch SwitchFromDynamic(dynamic val,
            dynamic status = null)
        {
            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main["switch"]["switch"].value ??
                    throw new ArgumentException("status.components.main.switch.switch.value is null!",
                    nameof(status));
                deviceStatus = status.components.main["switch"]["switch"].value;
            }

            return new LightSwitch()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value,
                CurrentState = deviceStatus != null ? SwitchStateFromDynamic(deviceStatus) : SwitchState.Unknown
            };
        }

        public static dynamic GetDeviceCommand(bool value)
        {
            var cmd = value ? "on" : "off";
            dynamic deviceCommands = JObject.Parse($@"{{
                ""commands"": [
		            {{
                        ""component"": ""main"",
                        ""capability"": ""switch"",
			            ""command"": ""{cmd}"",
			            ""arguments"": []
                    }}
                ]
            }}");

            return deviceCommands;
        }
    }
}
