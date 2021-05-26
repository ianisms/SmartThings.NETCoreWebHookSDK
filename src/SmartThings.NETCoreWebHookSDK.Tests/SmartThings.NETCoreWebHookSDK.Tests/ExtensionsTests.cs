using FluentAssertions;
using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void HttpExtensions_AbsoluteUri_Should_Not_Error()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "HTTP";
            httpContext.Request.Method = "GET";
            httpContext.Request.Path = "/";
            httpContext.Request.Host = new HostString("localhost");
            _ = httpContext.Request.AbsoluteUri();
        }

        [Fact]
        public void HttpExtensions_SetBasicAuthHeader_Should_Not_Error()
        {
            var httpRequestMsg = new HttpRequestMessage();

            httpRequestMsg.SetBasicAuthHeader("foo", "foo");
        }

        public static IEnumerable<object[]> InvalidBasicAuthData =>
           new List<object[]>
            {
                new object[] { null, "foo", "foo", "of missing request" },
                new object[] { new HttpRequestMessage(), null, "foo", "of missing user name" },
                new object[] { new HttpRequestMessage(), "foo", null, "of missing password" },
            };

        [Theory]
        [MemberData(nameof(InvalidBasicAuthData))]
        public void HttpExtensions_SetBasicAuthHeader_Should_Error(HttpRequestMessage httpRequestMessage,
            string userName,
            string password,
            string description)
        { 

            Action act = () =>
            {
                httpRequestMessage.SetBasicAuthHeader(userName, password);
            };
            act.Should().Throw<ArgumentNullException>(description);
        }

        public static IEnumerable<object[]> InvalidBearerAuthData =>
           new List<object[]>
            {
                new object[] { null, "foo",  "of missing request" },
                new object[] { new HttpRequestMessage(), null, "of missing tokenValue" }
            };

        [Theory]
        [MemberData(nameof(InvalidBearerAuthData))]
        public void HttpExtensions_SetBearerAuthHeader_Should_Error(HttpRequestMessage httpRequestMessage,
            string tokenValue,
            string description)
        {

            Action act = () =>
            {
                httpRequestMessage.SetBearerAuthHeader(tokenValue);
            };
            act.Should().Throw<ArgumentNullException>(description);
        }

        [Fact]
        public void HttpExtensions_ToStringContent_Should_Not_Error()
        {
            JObject foo = new();
            _ = foo.ToStringContent();
        }

        [Fact]
        public void HttpExtensions_ToStringContent_Null_Object_Should_Error()
        {
            Action act = () =>
            {
                JObject foo = null;
                _ = foo.ToStringContent();
            };
            act.Should().Throw<ArgumentNullException>("of null object");
        }

        [Fact]
        public void ObjectExtensions_ToJson_Should_Not_Error()
        {
            object foo = new();
            _ = foo.ToJson();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddWebhookHandlers_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddWebhookHandlers();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddInstalledAppTokenManager_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddInstalledAppTokenManager();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddInstalledAppTokenManagerService_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddInstalledAppTokenManagerService();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddFileBackedInstalledAppManager_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddFileBackedInstalledAppManager();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddAzureStorageBackedInstalledAppManager_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddAzureStorageBackedInstalledAppManager();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddFileBackedStateManager_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddFileBackedStateManager<string>();
        }

        [Fact]
        public void ServiceCollectionExtensions_AddAzureStorageStateManager_Should_Not_Error()
        {
            var services = new ServiceCollection();
            services.AddAzureStorageStateManager<string>();
        }
    }
}
