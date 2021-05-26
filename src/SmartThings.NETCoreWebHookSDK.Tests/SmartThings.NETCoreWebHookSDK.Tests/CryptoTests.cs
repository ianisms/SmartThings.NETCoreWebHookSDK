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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;
using System;
using System.Net.Http;
using Xunit;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Moq.Protected;
using Moq.Contrib.HttpClient;
using System.Threading;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class CryptoTests
    {
        private readonly Mock<ILogger<CryptoUtils>> _mockLogger;
        private readonly Mock<IOptions<CryptoUtilsConfig>> _mockOptions;
        private readonly Mock<IOptions<CryptoUtilsConfig>> _mockBadOptions;
        private readonly Mock<CryptoUtilsConfig> _mockConfig;
        private readonly Mock<CryptoUtilsConfigValidator> _mockCryptoUtilsConfigValidator;
        private readonly HttpClient _httpClient;

        public CryptoTests()
        {
            _mockLogger = new Mock<ILogger<CryptoUtils>>();
            _mockOptions = new Mock<IOptions<CryptoUtilsConfig>>();
            _mockBadOptions = new Mock<IOptions<CryptoUtilsConfig>>();
            _mockConfig = new Mock<CryptoUtilsConfig>();
            _mockCryptoUtilsConfigValidator = new Mock<CryptoUtilsConfigValidator>();

            _mockOptions.SetupGet(m => m.Value)
                .Returns(_mockConfig.Object);

            var handler = new Mock<HttpMessageHandler>();

            handler.Protected().As<IHttpMessageHandler>()
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new ByteArrayContent(Array.Empty<byte>())
                });

            _httpClient = new HttpClient(handler.Object, false);
        }

        [Fact]
        public void CryptoUtilsCtor_Should_Not_Error()
        {
            _ = new CryptoUtils(_mockLogger.Object,
                _mockOptions.Object,
                _mockCryptoUtilsConfigValidator.Object,
                _httpClient);
        }

        [Fact]
        public void CryptoUtilsCtor_Missing_Args_Should_Throw()
        {
            Action ctorCall = () =>
            {
                var utils = new CryptoUtils(null,
                    _mockOptions.Object,
                    _mockCryptoUtilsConfigValidator.Object,
                    _httpClient);
            };
            ctorCall.Should().Throw<ArgumentNullException>("Of missing logger");

            ctorCall = () =>
            {
                var utils = new CryptoUtils(_mockLogger.Object,
                    null,
                    _mockCryptoUtilsConfigValidator.Object,
                    _httpClient);
            };
            ctorCall.Should().Throw<ArgumentNullException>("Of missing options");

            ctorCall = () =>
            {
                var utils = new CryptoUtils(_mockLogger.Object,
                    _mockBadOptions.Object,
                    _mockCryptoUtilsConfigValidator.Object,
                    _httpClient);
            };
            ctorCall.Should().Throw<ArgumentNullException>("Of missing options.value");

            ctorCall = () =>
            {
                var utils = new CryptoUtils(_mockLogger.Object,
                    _mockOptions.Object,
                    null,
                    _httpClient);
            };
            ctorCall.Should().Throw<ArgumentNullException>("Of missing validator");

            ctorCall = () =>
            {
                var utils = new CryptoUtils(_mockLogger.Object,
                    _mockOptions.Object,
                    _mockCryptoUtilsConfigValidator.Object,
                    null);
            };
            ctorCall.Should().Throw<ArgumentNullException>("Of missing http client");
        }

        [Fact]
        public async Task VerifySignedRequestAsync_Valid_Sig_Should_Not_Throw_Unexpected()
        {
            var headerVal = "Signature keyId=\"/SmartThings/89:94:9a:9a:51:24:2d:6d:40:21:63:44:9c:b1:88:14\",signature=\"Zm9v\",headers=\"(request-target) digest date\",algorithm=\"rsa-sha256\"";
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Method = "GET";
            httpContext.Request.Path = "/";
            httpContext.Request.Headers.Add("Authorization", headerVal);
            httpContext.Request.Headers.Add("(request-target)", $"get /");
            httpContext.Request.Headers.Add("digest", "foo");
            httpContext.Request.Headers.Add("date", $"{DateTime.UtcNow.ToFileTimeUtc()}");

            var utils = new CryptoUtils(_mockLogger.Object,
                _mockOptions.Object,
                _mockCryptoUtilsConfigValidator.Object,
                _httpClient);
            _ = await utils.VerifySignedRequestAsync(httpContext.Request);
        }

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
