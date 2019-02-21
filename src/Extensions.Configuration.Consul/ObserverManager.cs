using System.Collections.Generic;
using Consul;
using Microsoft.Extensions.Logging;

namespace Extensions.Configuration.Consul
{
    internal static class ObserverManager
    {
        private static IObserver Observer { get; set; }

        public static ConsulAgentConfiguration Configuration { get; private set; }

        public static void Attach(IObserver observer, ConsulAgentConfiguration configuration)
        {
            Observer = observer;
            Configuration = configuration;
        }

        public static void Notify(List<KVPair> kVPairs, ILogger logger)
        {
            Observer.OnChange(kVPairs, logger);
        }
    }
}
