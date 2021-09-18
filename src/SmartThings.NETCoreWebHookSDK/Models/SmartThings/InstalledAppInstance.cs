#region Copyright
// <copyright file="InstalledAppInstance.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using Newtonsoft.Json;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public class InstalledAppInstance
    {
        public string InstalledAppId { get; set; }
        [JsonProperty(PropertyName = "accessToken")]
        public Token AccessToken { get; private set; }
        [JsonProperty(PropertyName = "refreshToken")]
        public Token RefreshToken { get; private set; }
        public Location InstalledLocation { get; set; }

        public void SetTokens(string authToken,
            string refreshToken = null,
            long expiresIn = long.MinValue)
        {
            _ = authToken ?? throw new ArgumentNullException(nameof(authToken));

            var rationalizedExpiresIn = Token.AccessTokenTTL;
            if (expiresIn != long.MinValue)
            {
                rationalizedExpiresIn = new TimeSpan(expiresIn).Subtract(
                    TimeSpan.FromMilliseconds(10)); // buffer
            }

            var now = DateTime.Now;
            var atExpiresDt = now.Add(rationalizedExpiresIn);

            AccessToken = new Token()
            {
                TokenType = OAuthTokenType.AccessToken,
                TokenValue = authToken,
                ExpiresDT = atExpiresDt
            };

            if (refreshToken != null)
            {
                var rtExpiresDt = now.Add(Token.RefreshTokenTTL);

                RefreshToken = new Token()
                {
                    TokenType = OAuthTokenType.RefreshToken,
                    TokenValue = refreshToken,
                    ExpiresDT = rtExpiresDt
                };
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is InstalledAppInstance))
            {
                return false;
            }

            var targetObj = (obj as InstalledAppInstance);

            return this.InstalledAppId.Equals(targetObj.InstalledAppId, StringComparison.Ordinal) &&
                this.AccessToken.Equals(targetObj.AccessToken) &&
                this.RefreshToken.Equals(targetObj.RefreshToken) &&
                this.InstalledLocation.Equals(targetObj.InstalledLocation);
        }

        public override int GetHashCode()
        {
            return this.InstalledAppId.GetHashCode(StringComparison.Ordinal) +
                this.AccessToken.GetHashCode() +
                this.RefreshToken.GetHashCode() +
                this.InstalledLocation.GetHashCode();
        }
    }
}
