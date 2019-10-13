using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public enum OAuthTokenType { AccessToken, RefreshToken }

    public class Token
    {
        public static TimeSpan AccessTokenTTL = TimeSpan.FromMinutes(4.5);
        public static TimeSpan RefreshTokenTTL = TimeSpan.FromMinutes(29.5);

        public OAuthTokenType TokenType { get; set; }
        public string TokenValue { get; set; }
        public DateTime ExpiresDT { get; set; }

        public bool IsExpired 
        {
            get
            {
                return ExpiresDT < DateTime.Now;
            }
        }
    }
}
