#region Copyright
// <copyright file="InMemoryStateManager.cs" company="Ian N. Bennett">
//
// Copyright (C) 2019 Ian N. Bennett
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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ianisms.SmartThings.NETCoreWebHookSDK.Utils.State
{
    public class InMemoryStateManager<T> : StateManager<T>
    {
        private Dictionary<string, T> stateCache { get; set; }

        public InMemoryStateManager(ILogger<IStateManager<T>> logger)
            : base(logger)
        {
        }

        public override async Task PersistCacheAsync()
        {
        }

        public override async Task LoadCacheAsync()
        {
            stateCache = new Dictionary<string, T>();
        }
    }
}
