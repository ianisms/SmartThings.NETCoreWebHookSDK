using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using Newtonsoft.Json;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings
{
    public class InstalledApp
    {
        public string InstalledAppId { get; set; } = null;
        [JsonProperty(PropertyName = "accessToken")]
        public Token AccessToken { get; private set; } = null;
        [JsonProperty(PropertyName = "refreshToken")]
        public Token RefreshToken { get; private set; } = null;
        public Location InstalledLocation { get; set; } = null;

        public void SetTokens(string authToken,
            string refreshToken = null,
            long expiresIn = long.MinValue)
        {
            _ = authToken ?? throw new ArgumentNullException(nameof(authToken));

            var rationalizedExpiresIn = TimeSpan.FromMinutes(4.5);
            if (expiresIn != long.MinValue)
            {
                rationalizedExpiresIn = new TimeSpan(expiresIn).Subtract(
                    TimeSpan.FromMilliseconds(10)); // buffer
            }

            var now = DateTime.Now;
            var atExpiresDt = now.Add(rationalizedExpiresIn);

            AccessToken = new Token()
            {
                TokenType = Token.OAuthTokenType.AccessToken,
                TokenValue = authToken,
                ExpiresDT = atExpiresDt
            };

            if (refreshToken != null)
            {
                var rtExpiresDt = now.Add(TimeSpan.FromDays(29));

                RefreshToken = new Token()
                {
                    TokenType = Token.OAuthTokenType.RefreshToken,
                    TokenValue = refreshToken,
                    ExpiresDT = rtExpiresDt
                };
            }
        }
    }
}
