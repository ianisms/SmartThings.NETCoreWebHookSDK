#region Copyright
// <copyright file="DoorLock.cs" company="Ian N. Bennett">
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
                    throw new ArgumentException("status.components.main.lock.lock.value is null!",
                    nameof(status));
                status = status.components.main["lock"]["lock"].value;
            }

            var val = status.Value.ToLowerInvariant();

            var state = LockState.Unknown;
            if (!Enum.TryParse<LockState>(val, true, out state))
            {
                throw new ArgumentException($"DoorLock.LockStateFromDynamic status is an invalid value {status}",
                    nameof(status));
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
                    throw new ArgumentException("status.components.main.lock.lock.value is null!",
                    nameof(status));
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

        public override bool Equals(object obj)
        {
            if (!(obj is DoorLock))
            {
                return false;
            }

            var targetObj = (obj as DoorLock);

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
