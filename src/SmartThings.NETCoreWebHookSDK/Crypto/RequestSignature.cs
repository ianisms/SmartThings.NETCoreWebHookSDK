#region Copyright
// <copyright file="RequestSignature.cs" company="Ian N. Bennett">
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
