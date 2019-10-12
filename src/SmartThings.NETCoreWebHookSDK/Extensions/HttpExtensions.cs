using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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
