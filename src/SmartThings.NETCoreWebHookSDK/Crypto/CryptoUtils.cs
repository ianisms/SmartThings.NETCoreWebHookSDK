using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
            CspParameters cspParameters = new CspParameters();
            cspParameters.KeyContainerName = "MyKeyContainer";
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParameters);

            var reader = new StringReader(key);
            var pem = new PemReader(reader);
            var kp = pem.ReadObject();

            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters((RsaKeyParameters)kp);
            rsaKey.ImportParameters(rsaParameters);
            return rsaKey;
        }

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
            var pubKeyContent = await this.config.GetPublicKeyContentAsync();
            this.publicKeyProvider = GetRSAProviderFromPem(pubKeyContent);
        }

        public async Task<bool> VerifySignedRequestAsync(HttpRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            if (publicKeyProvider == null)
            {
                await InitializeRSAProviderAsync();
            }

            var sig = RequestSignature.ParseFromHeaderVal(request.Headers["Authorization"].FirstOrDefault());

            var encoding = UTF8Encoding.UTF8;
            var sigBytes = Convert.FromBase64String(sig.Signature);
            var signVal = GetSignVal(sig, request);
            return publicKeyProvider.VerifyData(signVal, "SHA256", sigBytes);
        }
    }
}
