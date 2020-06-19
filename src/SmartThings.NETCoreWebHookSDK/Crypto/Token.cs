﻿#region Copyright
// <copyright file="Token.cs" company="Ian N. Bennett">
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
using System.Diagnostics.CodeAnalysis;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public enum OAuthTokenType { AccessToken, RefreshToken }

    [SuppressMessage("Microsoft.Naming", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "Can't use const with TimeSpan initialization")]
    public class Token
    {
        public static TimeSpan AccessTokenTTL = TimeSpan.FromMinutes(4.5);
        public static TimeSpan RefreshTokenTTL = TimeSpan.FromMinutes(29.5);

        public OAuthTokenType TokenType { get; set; }
        public string TokenValue { get; set; }
        public DateTime ExpiresDT { get; set; }

        public bool IsExpired
        {
            get
            {
                return ExpiresDT < DateTime.Now;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Token))
            {
                return false;
            }

            var targetObj = (obj as Token);

            return this.TokenType.Equals(targetObj.TokenType) &&
                this.TokenValue.Equals(targetObj.TokenValue, StringComparison.Ordinal) &&
                this.ExpiresDT.Equals(targetObj.ExpiresDT);
        }

        public override int GetHashCode()
        {
            return this.TokenType.GetHashCode() +
                this.TokenValue.GetHashCode(StringComparison.Ordinal) +
                this.ExpiresDT.GetHashCode();
        }
    }
}
