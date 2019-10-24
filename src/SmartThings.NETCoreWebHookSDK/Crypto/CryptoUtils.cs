#region Copyright
// <copyright file="CryptoUtils.cs" company="Ian N. Bennett">
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
