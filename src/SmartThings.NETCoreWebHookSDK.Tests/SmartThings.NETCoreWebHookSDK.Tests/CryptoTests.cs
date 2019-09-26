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
            var headerVal = "Signature keyId=\"/SmartThings/80:34:9f:9f:51:84:7d:6d:40:21:63:44:9c:b1:78:44\",signature=\"AhTJ9uHjYbjMTWbToZ0ueRWu6gM52ueBRUSEm2H4xEyr4+cQ7zVN3IcZYsva3Kx0HcXU+2qiBlTmu4KgGW4PcANNV7bI8DUz/cP6DNNyuGIveYQDe2XiG/gnphyxfJHbPoGGmWAZaJfHaiutAW7GnzOmjGOfvsNX0xT/PMOsUbyuJxMNw5btLrIYeenXQN4trD9GfudTljxaFRuFp/an3A23qC0hxmweGaek3Tzn65/iceWnok1gLDSo9IZCvGbjPc7UUZFzEdHNDsIOSHXMSwxLQntMl0UvfMytejT4p2X/yJXlqHlBd/GNwFGp4P8kj3iylMCXDEhbImx7dJ2Fnw==\",headers=\"(request-target) digest date\",algorithm=\"rsa-sha256\"";
            var sig = RequestSignature.ParseFromHeaderVal(headerVal);
            Assert.NotNull(sig);
            var expected = "/SmartThings/80:34:9f:9f:51:84:7d:6d:40:21:63:44:9c:b1:78:44";
            Assert.Equal(sig.KeyId, expected);
            expected = "AhTJ9uHjYbjMTWbToZ0ueRWu6gM52ueBRUSEm2H4xEyr4+cQ7zVN3IcZYsva3Kx0HcXU+2qiBlTmu4KgGW4PcANNV7bI8DUz/cP6DNNyuGIveYQDe2XiG/gnphyxfJHbPoGGmWAZaJfHaiutAW7GnzOmjGOfvsNX0xT/PMOsUbyuJxMNw5btLrIYeenXQN4trD9GfudTljxaFRuFp/an3A23qC0hxmweGaek3Tzn65/iceWnok1gLDSo9IZCvGbjPc7UUZFzEdHNDsIOSHXMSwxLQntMl0UvfMytejT4p2X/yJXlqHlBd/GNwFGp4P8kj3iylMCXDEhbImx7dJ2Fnw==";
            Assert.Equal(sig.Signature, expected);
            Assert.Contains("(request-target)", sig.Headers);
            Assert.Contains("digest", sig.Headers);
            Assert.Contains("date", sig.Headers);
            expected = "rsa-sha256";
            Assert.Equal(sig.Algorithm, expected);
        }
    }
}
