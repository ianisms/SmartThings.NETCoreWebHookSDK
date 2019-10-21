#region Copyright
// <copyright file="SpeechDevice.cs" company="Ian N. Bennett">
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

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public class SpeechDevice : BaseModel
    {
        public static SpeechDevice SpeechDeviceFromDynamic(dynamic val)
        {
            return new SpeechDevice()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value
            };
        }

        public static dynamic GetSpeakDeviceCommand(string message)
        {
            dynamic deviceCommands = JObject.Parse($@"{{
                ""commands"": [
		            {{
                        ""component"": ""main"",
                        ""capability"": ""speechSynthesis"",
			            ""command"": ""speak"",
			            ""arguments"": [""{message}""]
                    }}
                ]
            }}");

            return deviceCommands;
        }
    }
}
