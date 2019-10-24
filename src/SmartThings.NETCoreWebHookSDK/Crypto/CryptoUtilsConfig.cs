#region Copyright
// <copyright file="CryptoUtilsConfig.cs" company="Ian N. Bennett">
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
using System.IO;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public class CryptoUtilsConfig
    {
        [JsonProperty(Required = Required.Always)]
        public string PublicKeyFilePath { get; set; }

        internal async Task<string> GetPublicKeyContentAsync()
        {
            _ = PublicKeyFilePath ?? throw new InvalidOperationException($"{nameof(PublicKeyFilePath)} is null!");

            using var reader = File.OpenText(PublicKeyFilePath);

            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
