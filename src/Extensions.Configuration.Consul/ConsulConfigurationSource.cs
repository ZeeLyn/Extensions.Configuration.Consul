using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
    internal class ConsulConfigurationSource : IConfigurationSource
    {
        private ConsulAgentConfiguration Config { get; }

        public ConsulConfigurationSource(ConsulAgentConfiguration config)
        {
            Config = config;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var provider = new ConsulConfigurationProvider(Config);
            ObserverManager.Attach(provider, Config);
            return provider;
        }
    }
}
