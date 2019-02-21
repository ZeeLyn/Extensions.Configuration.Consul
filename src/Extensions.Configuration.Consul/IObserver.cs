using System.Collections.Generic;
using Consul;
using Microsoft.Extensions.Logging;

namespace Extensions.Configuration.Consul
{
    internal interface IObserver
    {
        void OnChange(List<KVPair> kVPairs, ILogger logger);
    }
}
