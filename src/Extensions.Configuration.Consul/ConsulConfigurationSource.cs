using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
	internal class ConsulConfigurationSource : IConfigurationSource
	{
		private ConsulAgentConfiguration Config { get; }

		private bool ReloadOnChange { get; }

		public ConsulConfigurationSource(ConsulAgentConfiguration config, bool reloadOnChange)
		{
			Config = config;
			ReloadOnChange = reloadOnChange;
		}

		public IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			return new ConsulConfigurationProvider(Config, ReloadOnChange);
		}
	}
}
