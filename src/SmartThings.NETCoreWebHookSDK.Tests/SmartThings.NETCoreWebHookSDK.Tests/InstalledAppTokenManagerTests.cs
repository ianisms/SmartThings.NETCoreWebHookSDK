#region Copyright
// <copyright file="InstalledAppTokenManagerTests.cs" company="Ian N. Bennett">
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

using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp;
using ianisms.SmartThings.NETCoreWebHookSDK.Utils.SmartThings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Tests
{
    public class InstalledAppTokenManagerTests : IDisposable
    {
        private bool disposed = false;
        private readonly Mock<ILogger<IInstalledAppTokenManager>> mockIALogger;
        private readonly Mock<IInstalledAppManager> mockIAManager;
        private readonly Mock<IOptions<InstalledAppTokenManagerConfig>> mockIAOptions;
        private readonly IInstalledAppTokenManager installedAppTokenManager;
        private readonly InstalledAppTokenManagerService installedAppTokenManagerService;

        public InstalledAppTokenManagerTests()
        {
            mockIALogger = new Mock<ILogger<IInstalledAppTokenManager>>();
            mockIAManager = new Mock<IInstalledAppManager>();

            var iaConfig = new InstalledAppTokenManagerConfig()
            {
                RefreshInterval = TimeSpan.FromSeconds(5)
            };

            mockIAOptions = new Mock<IOptions<InstalledAppTokenManagerConfig>>();
            mockIAOptions.Setup(opt => opt.Value)
                .Returns(new InstalledAppTokenManagerConfig()
                {
                    RefreshInterval = TimeSpan.FromSeconds(5)
                });

            installedAppTokenManager = new InstalledAppTokenManager(mockIALogger.Object,
                mockIAManager.Object);

            installedAppTokenManagerService = new InstalledAppTokenManagerService(mockIALogger.Object,
                mockIAManager.Object,
                mockIAOptions.Object);
        }

        [Fact]
        public async Task IAMRefreshAllITokensAsync_ShouldNotError()
        {
            await installedAppTokenManager.RefreshAllTokensAsync();
        }

        [Fact]
        public async Task IAMSRefreshAllStartAsync_ShouldNotError()
        {
            await installedAppTokenManagerService.StartAsync(CancellationToken.None);
        }

        [Fact]
        public async Task IAMSRefreshAllStopAsync_ShouldNotError()
        {
            await installedAppTokenManagerService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task IAMSRefreshAllTokensAsync_ShouldNotError()
        {
            await installedAppTokenManagerService.RefreshAllTokensAsync();
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    installedAppTokenManagerService.Dispose();
                }

                // Note disposing has been done.
                disposed = true;

            }
        }
    }
}
