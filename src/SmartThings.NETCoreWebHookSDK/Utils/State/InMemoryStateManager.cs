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
