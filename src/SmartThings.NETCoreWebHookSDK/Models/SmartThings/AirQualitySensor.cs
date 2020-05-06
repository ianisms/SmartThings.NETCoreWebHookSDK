#region Copyright
// <copyright file="AirQualitySensor.cs" company="Ian N. Bennett">
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
