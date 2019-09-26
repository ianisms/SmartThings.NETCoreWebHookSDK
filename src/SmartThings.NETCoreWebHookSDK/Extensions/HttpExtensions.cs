using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
    }
}
