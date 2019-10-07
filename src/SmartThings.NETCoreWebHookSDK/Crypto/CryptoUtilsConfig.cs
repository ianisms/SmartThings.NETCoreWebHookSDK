using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public class CryptoUtilsConfig
    {
        [JsonProperty(Required = Required.Always)]
        public string PublicKeyFilePath { get; set; }

        internal async Task<string> GetPublicKeyContentAsync()
        {
            _ = PublicKeyFilePath ?? throw new InvalidOperationException($"{nameof(PublicKeyFilePath)} is null!");

            using (var reader = File.OpenText(PublicKeyFilePath))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
