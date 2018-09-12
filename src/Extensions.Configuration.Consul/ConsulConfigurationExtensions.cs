using System;
using System.Collections.Generic;
using System.Linq;
using Consul;
using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
	public static class ConsulConfigurationExtensions
	{
		private static List<ConsulAgentConfiguration> Configs { get; }

		static ConsulConfigurationExtensions()
		{
			Configs = new List<ConsulAgentConfiguration>();
		}
		public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, ConsulClientConfiguration consulClientConfiguration, ConsulQueryOptions queryOptions, bool reloadOnChange = false)
		{
			if (consulClientConfiguration == null)
				throw new ArgumentNullException(nameof(consulClientConfiguration), "The agent url can't be null.");
			Configs.Add(new ConsulAgentConfiguration { ClientConfiguration = consulClientConfiguration, QueryOptions = queryOptions });
			return Add(configurationBuilder, reloadOnChange);
		}


		public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, string agentUrl, string token = "", string prefix = "", string dataCenter = "", bool reloadOnChange = false)
		{
			if (string.IsNullOrWhiteSpace(agentUrl))
				throw new ArgumentNullException(nameof(agentUrl), "The agent url can't be null.");
			Configs.Add(new ConsulAgentConfiguration
			{
				ClientConfiguration = new ConsulClientConfiguration
				{
					Address = new Uri(agentUrl),
					Token = token,
					Datacenter = dataCenter
				},
				QueryOptions = new ConsulQueryOptions
				{
					Prefix = prefix
				}
			});
			return Add(configurationBuilder, reloadOnChange);
		}

		public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, IEnumerable<ConsulAgentConfiguration> configurations, bool reloadOnChange = false)
		{
			var consulAgentConfigurations = configurations.ToList();
			if (configurations == null || !consulAgentConfigurations.Any())
				throw new ArgumentNullException(nameof(configurations), "The agent can't be null.");
			Configs.AddRange(consulAgentConfigurations);
			return Add(configurationBuilder, reloadOnChange);
		}

		private static IConfigurationBuilder Add(IConfigurationBuilder configurationBuilder, bool reloadOnChange)
		{
			return configurationBuilder.Add(new ConsulConfigurationSource(Configs, reloadOnChange));
		}
	}
}
