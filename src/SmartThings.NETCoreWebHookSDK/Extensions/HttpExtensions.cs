#region Copyright
// <copyright file="HttpExtensions.cs" company="Ian N. Bennett">
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

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Extensions
{
    public static class HttpExtensions
    {
        public static Uri AbsoluteUri(this HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port.GetValueOrDefault(80),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder.Uri;
        }

        public static StringContent ToStringContent(this JObject payload)
        {
            _ = payload ?? throw new ArgumentNullException(nameof(payload));
            return new StringContent(payload.ToString());
        }
        public static void SetBasicAuthHeader(this HttpRequestMessage request, string username, string password)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes(
                            $"{username}:{password}")));
        }
        public static void SetBearerAuthHeader(this HttpRequestMessage request, string tokenValue)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenValue);
        }
    }
}
