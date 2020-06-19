#region Copyright
// <copyright file="BaseModel.cs" company="Ian N. Bennett">
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
