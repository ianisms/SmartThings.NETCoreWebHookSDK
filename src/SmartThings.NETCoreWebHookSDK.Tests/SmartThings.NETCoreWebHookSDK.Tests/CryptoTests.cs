using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using System;
using System.Collections.Generic;
using Xunit;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class CryptoTests
    {
        [Fact]
        public void RequestSignatureParseFromHeaderValParsesHeaderCorrectly()
        {
            var headerVal = "Signature keyId=\"/SmartThings/89:94:9a:9a:51:24:2d:6d:40:21:63:44:9c:b1:88:14\",signature=\"jskjdfhksdjhf!-9807987bkHKGHKu6gM52ueBRUSEm2H4xEyr4+cQ7zVN-87687HJHjgjhgJGJjj87687HJHjgjhgJGJjjNV7bI8DUz/c-87687HJHjgjhgJGJjjiG/-87687HJHjgjhgJGJjjfHaiutAW7GnzOmjGOfvsNX0xT/PMO-87687HJHjgjhgJGJjjtrD9GfudTljxaFRuFp/-87687HJHjgjhgJGJjj/-87687HJHjgjhgJGJjj-87687HJHjgjhgJGJjjDsIOSHXMSwxLQntMl0UvfMytejT4p2X/yJXlqHlBd/-87687HJHjgjhgJGJjjImx7dJ2Fnw==\",headers=\"(request-target) digest date\",algorithm=\"rsa-sha256\"";
            var sig = RequestSignature.ParseFromHeaderVal(headerVal);
            Assert.NotNull(sig);
            var expected = "/SmartThings/89:94:9a:9a:51:24:2d:6d:40:21:63:44:9c:b1:88:14";
            Assert.Equal(sig.KeyId, expected);
            expected = "jskjdfhksdjhf!-9807987bkHKGHKu6gM52ueBRUSEm2H4xEyr4+cQ7zVN-87687HJHjgjhgJGJjj87687HJHjgjhgJGJjjNV7bI8DUz/c-87687HJHjgjhgJGJjjiG/-87687HJHjgjhgJGJjjfHaiutAW7GnzOmjGOfvsNX0xT/PMO-87687HJHjgjhgJGJjjtrD9GfudTljxaFRuFp/-87687HJHjgjhgJGJjj/-87687HJHjgjhgJGJjj-87687HJHjgjhgJGJjjDsIOSHXMSwxLQntMl0UvfMytejT4p2X/yJXlqHlBd/-87687HJHjgjhgJGJjjImx7dJ2Fnw==";
            Assert.Equal(sig.Signature, expected);
            Assert.Contains("(request-target)", sig.Headers);
            Assert.Contains("digest", sig.Headers);
            Assert.Contains("date", sig.Headers);
            expected = "rsa-sha256";
            Assert.Equal(sig.Algorithm, expected);
        }
    }
}
