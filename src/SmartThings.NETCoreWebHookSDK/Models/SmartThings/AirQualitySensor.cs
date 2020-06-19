#region Copyright
// <copyright file="AirQualitySensor.cs" company="Ian N. Bennett">
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

    public class AirQualitySensor : BaseModel
    {
        public int CurrentValue { get; set; } = -1;

        public static AirQualitySensor AirQualitySensorFromDynamic(dynamic val,
            dynamic status = null)
        {
            dynamic deviceStatus = null;

            if (status != null)
            {
                _ = status.components.main.airQualitySensor.airQuality.value ??
                    throw new ArgumentException("status.components.main.airQualitySensor.airQuality.value is null!",
                    nameof(status));
                deviceStatus = status.components.main.airQualitySensor.airQuality.value;
            }

            return new AirQualitySensor()
            {
                Id = val.deviceId.Value,
                Label = val.label.Value,
                CurrentValue = deviceStatus != null ? int.Parse(deviceStatus.Value) : -1
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AirQualitySensor))
            {
                return false;
            }

            var targetObj = (obj as AirQualitySensor);

            return base.Equals(obj) &&
                this.CurrentValue.Equals(targetObj.CurrentValue);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() +
                this.CurrentValue.GetHashCode();
        }
    }
}
