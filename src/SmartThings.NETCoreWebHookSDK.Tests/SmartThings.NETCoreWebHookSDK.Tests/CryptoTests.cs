#region Copyright
// <copyright file="CryptoTests.cs" company="Ian N. Bennett">
//
// Copyright (C) 2020 Ian N. Bennett
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

using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
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
