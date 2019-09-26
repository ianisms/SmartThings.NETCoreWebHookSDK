using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public class RequestSignature
    {
        private const string SIGHEADERSTART = "Signature ";
        private static readonly string[] MUSTHAVEKEYS = { "keyId", "signature", "headers", "algorithm" };

        public string KeyId { get; private set; }
        public string Signature { get; private set; }
        public IEnumerable<string> Headers { get; private set; }
        public string Algorithm { get; private set; }


        public static RequestSignature ParseFromHeaderVal(string headerVal)
        {
            _ = headerVal ?? throw new ArgumentNullException(nameof(headerVal));

            if (!headerVal.StartsWith(SIGHEADERSTART, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Invalid auth header!  Must start with {SIGHEADERSTART}", nameof(headerVal));
            }

            headerVal = headerVal.Substring(SIGHEADERSTART.Length);

            var sigMap = headerVal.Split(',')
                .Select(part => part.Split('=', 2))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1].Replace("\"", "", StringComparison.OrdinalIgnoreCase));

            foreach (var mustHaveKey in MUSTHAVEKEYS)
            {
                if (!sigMap.ContainsKey(mustHaveKey))
                {
                    throw new ArgumentException($"Invalid auth header!  Missing {mustHaveKey} param!", nameof(headerVal));
                }
            }

            return new RequestSignature()
            {
                KeyId = sigMap["keyId"],
                Signature = sigMap["signature"],
                Headers = sigMap["headers"].Split(' '),
                Algorithm = sigMap["algorithm"]
            };
        }
    }
}
