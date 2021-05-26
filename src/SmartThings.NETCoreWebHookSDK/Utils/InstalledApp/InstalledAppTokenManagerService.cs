#region Copyright
// <copyright file="InstalledAppTokenManagerService.cs" company="Ian N. Bennett">
// MIT License
//
// Copyright (C) 2020 Ian N. Bennett
// 
// This file is part of SmartThings.NETCoreWebHookSDK
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
#endregion
using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public class InstalledAppTokenManagerService : InstalledAppTokenManager, IHostedService, IDisposable
    {
        private bool disposed = false;
        private Timer refreshTimer;
        private readonly ILogger<IInstalledAppTokenManager> _logger;
        private readonly InstalledAppTokenManagerConfig _installedAppTokenManagerConfig;

        public InstalledAppTokenManagerService(ILogger<IInstalledAppTokenManager> logger,
            IInstalledAppManager installedAppManager,
            IOptions<InstalledAppTokenManagerConfig> options)
            : base(logger, installedAppManager)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _installedAppTokenManagerConfig = options?.Value ??
                throw new ArgumentNullException(nameof(options));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync(CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            refreshTimer = new Timer(async (object state) =>
                {
                    await RefreshAllTokensAsync().ConfigureAwait(false);
                },
                null,
                TimeSpan.Zero,
                _installedAppTokenManagerConfig.RefreshInterval);

            _logger.LogDebug("InstalledAppTokenManagerService started...");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync(CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _logger.LogDebug("InstalledAppTokenManagerService stopped...");
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
                    if (refreshTimer != null)
                    {
                        // Dispose managed resources.
                        refreshTimer.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;

            }
        }
    }
}
