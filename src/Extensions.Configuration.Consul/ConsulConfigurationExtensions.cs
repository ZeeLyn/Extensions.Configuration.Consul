using System;
using System.Collections.Generic;
using System.Linq;
using Consul;
using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
	public static class ConsulConfigurationExtensions
	{

		public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, ConsulClientConfiguration consulClientConfiguration, ConsulQueryOptions queryOptions, bool reloadOnChange = false)
		{
			if (consulClientConfiguration == null)
				throw new ArgumentNullException(nameof(consulClientConfiguration), "The agent url can't be null.");
			return Add(configurationBuilder, new ConsulAgentConfiguration { ClientConfiguration = consulClientConfiguration, QueryOptions = queryOptions }, reloadOnChange);
		}


		public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, string agentUrl, string token = "", string prefix = "", string dataCenter = "", bool reloadOnChange = false)
		{
			if (string.IsNullOrWhiteSpace(agentUrl))
				throw new ArgumentNullException(nameof(agentUrl), "The agent url can't be null.");
			return Add(configurationBuilder, new ConsulAgentConfiguration
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
			}, reloadOnChange);
		}

		private static IConfigurationBuilder Add(IConfigurationBuilder configurationBuilder, ConsulAgentConfiguration configuration, bool reloadOnChange)
		{
			return configurationBuilder.Add(new ConsulConfigurationSource(configuration, reloadOnChange));
		}
	}
}
