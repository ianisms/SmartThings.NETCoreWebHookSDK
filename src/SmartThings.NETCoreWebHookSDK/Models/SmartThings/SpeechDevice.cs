#region Copyright
// <copyright file="SpeechDevice.cs" company="Ian N. Bennett">
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
    [Obsolete("No longer supported by SmartThings", false)]
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
