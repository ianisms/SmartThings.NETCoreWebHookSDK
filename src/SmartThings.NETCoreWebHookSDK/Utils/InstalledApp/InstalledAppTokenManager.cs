#region Copyright
// <copyright file="InstalledAppTokenManager.cs" company="Ian N. Bennett">
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
using ianisms.SmartThings.NETCoreWebHookSDK.Models.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.InstalledApp
{
    public interface IInstalledAppTokenManager
    {
        ILogger<IInstalledAppTokenManager> Logger { get; }
        IInstalledAppManager InstalledAppManager { get; }
        InstalledAppTokenManagerConfig InstalledAppTokenManagerConfig { get; }
        Task RefreshAllTokensAsync();
    }

    public class InstalledAppTokenManager : IInstalledAppTokenManager
    {
        public ILogger<IInstalledAppTokenManager> Logger { get; private set; }
        public IInstalledAppManager InstalledAppManager { get; private set; }
        public InstalledAppTokenManagerConfig InstalledAppTokenManagerConfig { get; private set; }

        public InstalledAppTokenManager(ILogger<IInstalledAppTokenManager> logger,
            IInstalledAppManager installedAppManager,
            IOptions<InstalledAppTokenManagerConfig> options)
        {
            _ = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ = installedAppManager ??
                throw new ArgumentNullException(nameof(installedAppManager));

            this.Logger = logger;
            this.InstalledAppManager = installedAppManager;

            this.InstalledAppTokenManagerConfig = 
                options != null ? options.Value : 
                new InstalledAppTokenManagerConfig();
        }

        public async Task RefreshAllTokensAsync()
        {
            Logger.LogDebug("Refreshing all tokens...");

            try
            {
                await InstalledAppManager.RefreshAllInstalledAppTokensAsync().ConfigureAwait(false);
                Logger.LogDebug("All tokens refreshed...");
            }
            catch (AggregateException ex)
            {
                Logger.LogError(ex, "Exception trying to refresh all tokens!");
            }
        }
    }
}
