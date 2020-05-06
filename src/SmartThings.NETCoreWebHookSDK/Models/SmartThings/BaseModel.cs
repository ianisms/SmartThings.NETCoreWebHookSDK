#region Copyright
// <copyright file="BaseModel.cs" company="Ian N. Bennett">
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

using Newtonsoft.Json;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public abstract class BaseModel
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is BaseModel))
            {
                return false;
            }

            var targetObj = (obj as BaseModel);

            return this.Id.Equals(targetObj.Id, StringComparison.Ordinal) &&
                this.Label.Equals(targetObj.Label, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode(StringComparison.Ordinal) +
                this.Label.GetHashCode(StringComparison.Ordinal);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
