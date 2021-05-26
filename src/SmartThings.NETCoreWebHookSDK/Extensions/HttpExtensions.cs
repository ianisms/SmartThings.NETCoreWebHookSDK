#region Copyright
// <copyright file="HttpExtensions.cs" company="Ian N. Bennett">
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
            _ = username ?? throw new ArgumentNullException(nameof(username));
            _ = password ?? throw new ArgumentNullException(nameof(password));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes(
                            $"{username}:{password}")));
        }

        public static void SetBearerAuthHeader(this HttpRequestMessage request, string tokenValue)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            _ = tokenValue ?? throw new ArgumentNullException(nameof(tokenValue));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenValue);
        }
    }
}
