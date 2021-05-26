#region Copyright
// <copyright file="CryptoUtils.cs" company="Ian N. Bennett">
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
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public interface ICryptoUtils
    {
        Task<bool> VerifySignedRequestAsync(HttpRequest request);
    }

    public class CryptoUtils : ICryptoUtils
    {
        private readonly ILogger<CryptoUtils> _logger;
        private readonly CryptoUtilsConfig _config;
        private readonly CryptoUtilsConfigValidator _cryptoUtilsConfigValidator;
        private readonly HttpClient _httpClient;
        private RSACryptoServiceProvider _publicKeyProvider;

        public CryptoUtils(ILogger<CryptoUtils> logger,
            IOptions<CryptoUtilsConfig> options,
            CryptoUtilsConfigValidator cryptoUtilsConfigValidator,
            HttpClient httpClient)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _config = options?.Value ??
                throw new ArgumentNullException(nameof(options));
            _cryptoUtilsConfigValidator = cryptoUtilsConfigValidator ??
                throw new ArgumentNullException(nameof(cryptoUtilsConfigValidator));
            _httpClient = httpClient ??
                throw new ArgumentNullException(nameof(httpClient));

            _cryptoUtilsConfigValidator.ValidateAndThrow(_config);
        }

        private async Task<RSACryptoServiceProvider> GetRSAProviderFromCertAsync(RequestSignature reqSignature)
        {
            _ = reqSignature ?? throw new ArgumentNullException(nameof(reqSignature));
            _ = reqSignature.KeyId ?? throw new InvalidOperationException($"{nameof(reqSignature.KeyId)} is null");

            var certUri = $"{_config.SmartThingsCertUriRoot}/{reqSignature.KeyId}";
            var certBytes = await _httpClient.GetByteArrayAsync(certUri);
            if (certBytes?.Length > 0)
            {
                var cert = new X509Certificate2(certBytes);
                return (RSACryptoServiceProvider)cert.PublicKey.Key;
            }

            return null;
        }

        private static byte[] GetHash(RequestSignature reqSignature, HttpRequest request)
        {
            var siginingString = new StringBuilder();
            int headerCount = reqSignature.Headers.Count();
            for (int i = 0; i < headerCount; i++)
            {
                var h = reqSignature.Headers.ElementAt(i);

                if (h == "(request-target)")
                {
                    siginingString.Append("(request-target): ");
                    siginingString.Append(request.Method.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
                    siginingString.Append(' ');
                    siginingString.Append(request.Path);
                }
                else
                {
                    if (!request.Headers.TryGetValue(h, out StringValues vals))
                    {
                        throw new InvalidOperationException($"Missing {h} header per the auth header!");
                    }

                    siginingString.Append(h);
                    siginingString.Append(": ");
                    siginingString.Append(vals.FirstOrDefault());
                }
                if (i < (headerCount - 1))
                {
                    siginingString.Append('\n');
                }
            }

            var val = siginingString.ToString();
            var encoding = UTF8Encoding.UTF8;
            return encoding.GetBytes(val);
        }

        public async Task<bool> VerifySignedRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            _logger.LogDebug($"Verifying sign request: {request.Path}");

            var reqSignature = RequestSignature.ParseFromHeaderVal(request.Headers["Authorization"].FirstOrDefault());

            if (_publicKeyProvider == null)
            {
                _publicKeyProvider = await GetRSAProviderFromCertAsync(reqSignature);
            }

            var hash = GetHash(reqSignature, request);
            var data = Convert.FromBase64String(reqSignature.Signature);

            if (_publicKeyProvider != null)
            {
                return _publicKeyProvider.VerifyData(hash, "SHA256", data);
            } else
            {
                return false;
            }
        }
    }
}
