#region Copyright
// <copyright file="InstalledApp.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using Newtonsoft.Json;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public class InstalledApp
    {
        public string InstalledAppId { get; set; } = null;
        [JsonProperty(PropertyName = "accessToken")]
        public Token AccessToken { get; private set; } = null;
        [JsonProperty(PropertyName = "refreshToken")]
        public Token RefreshToken { get; private set; } = null;
        public Location InstalledLocation { get; set; } = null;

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
    }
}
