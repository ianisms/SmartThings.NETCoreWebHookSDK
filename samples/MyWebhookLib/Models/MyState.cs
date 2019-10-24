#region Copyright
// <copyright file="MyState.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyWebhookLib.Models
{
    public class MyState
    {
        public string InstalledAppId { get; set; }
        public bool IsAppEnabled { get; set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<LightSwitch> LightSwitches { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
