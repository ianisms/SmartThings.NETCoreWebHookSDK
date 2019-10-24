#region Copyright
// <copyright file="RequestSignature.cs" company="Ian N. Bennett">
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
using System.Collections.Generic;
using System.Linq;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public class RequestSignature
    {
        private const string SIGHEADERSTART = "Signature ";
        private static readonly string[] MUSTHAVEKEYS = { "keyId", "signature", "headers", "algorithm" };

        public string KeyId { get; private set; }
        public string Signature { get; private set; }
        public IEnumerable<string> Headers { get; private set; }
        public string Algorithm { get; private set; }


        public static RequestSignature ParseFromHeaderVal(string headerVal)
        {
            _ = headerVal ?? throw new ArgumentNullException(nameof(headerVal));

            if (!headerVal.StartsWith(SIGHEADERSTART, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Invalid auth header!  Must start with {SIGHEADERSTART}",
                    nameof(headerVal));
            }

            headerVal = headerVal.Substring(SIGHEADERSTART.Length);

            var sigMap = headerVal.Split(',')
                .Select(part => part.Split('=', 2))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1].Replace("\"", "", StringComparison.OrdinalIgnoreCase));

            foreach (var mustHaveKey in MUSTHAVEKEYS)
            {
                if (!sigMap.ContainsKey(mustHaveKey))
                {
                    throw new ArgumentException($"Invalid auth header!  Missing {mustHaveKey} param!",
                        nameof(headerVal));
                }
            }

            return new RequestSignature()
            {
                KeyId = sigMap["keyId"],
                Signature = sigMap["signature"],
                Headers = sigMap["headers"].Split(' '),
                Algorithm = sigMap["algorithm"]
            };
        }
    }
}
