using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using System;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Crypto
{
    public class Token
    {
        public enum OAuthTokenType { AccessToken, RefreshToken }

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
