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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly ILogger<CryptoUtils> logger;
        private readonly CryptoUtilsConfig config;
        private RSACryptoServiceProvider publicKeyProvider;

        public CryptoUtils(ILogger<CryptoUtils> logger,
            IOptions<CryptoUtilsConfig> options)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = options ?? throw new ArgumentNullException(nameof(options));

            this.logger = logger;
            this.config = options.Value;
        }

        private static RSACryptoServiceProvider GetRSAProviderFromPem(String key)
        {
            CspParameters csp = new CspParameters();
            csp.KeyContainerName = "MyKeyContainer";
            csp.Flags = CspProviderFlags.CreateEphemeralKey;
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(csp);

            var reader = new StringReader(key);
            var pem = new PemReader(reader);
            var kp = pem.ReadObject();

            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters((RsaKeyParameters)kp);
            rsaKey.ImportParameters(rsaParameters);
            return rsaKey;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Sig headers need to be lowercase")]
        private static byte[] GetSignVal(RequestSignature sig, HttpRequest request)
        {
            var siginingString = new StringBuilder();
            int headerCount = sig.Headers.Count();
            for (int i = 0; i < headerCount; i++)
            {
                var h = sig.Headers.ElementAt(i);

                if (h == "(request-target)")
                {
                    siginingString.Append("(request-target): ");
                    siginingString.Append(request.Method.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
                    siginingString.Append(' ');
                    siginingString.Append(request.Path);
                }
                else
                {
                    StringValues vals;
                    if (!request.Headers.TryGetValue(h, out vals))
                    {
                        throw new InvalidOperationException($"Missing {h} header per the auth header!");
                    }

                    siginingString.Append(h);
                    siginingString.Append(": ");
                    siginingString.Append(vals.FirstOrDefault());
                }
                if (i < (headerCount - 1))
                {
                    siginingString.Append("\n");
                }
            }

            var val = siginingString.ToString();
            var encoding = UTF8Encoding.UTF8;
            return encoding.GetBytes(val);
        }

        private async Task InitializeRSAProviderAsync()
        {
            var pubKeyContent = await this.config.GetPublicKeyContentAsync().ConfigureAwait(false);
            this.publicKeyProvider = GetRSAProviderFromPem(pubKeyContent);
        }

        public async Task<bool> VerifySignedRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            if (publicKeyProvider == null)
            {
                await InitializeRSAProviderAsync().ConfigureAwait(false);
            }

            var sig = RequestSignature.ParseFromHeaderVal(request.Headers["Authorization"].FirstOrDefault());

            var encoding = UTF8Encoding.UTF8;
            var sigBytes = Convert.FromBase64String(sig.Signature);
            var signVal = GetSignVal(sig, request);
            return publicKeyProvider.VerifyData(signVal, "SHA256", sigBytes);
        }
    }
}
