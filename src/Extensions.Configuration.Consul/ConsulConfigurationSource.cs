using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
	public class ConsulConfigurationSource : IConfigurationSource
	{
		private IEnumerable<ConsulAgentConfiguration> Config { get; }

		private bool ReloadOnChange { get; }

		public ConsulConfigurationSource(IEnumerable<ConsulAgentConfiguration> config, bool reloadOnChange)
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
