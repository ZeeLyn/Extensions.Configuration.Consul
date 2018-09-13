using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
	public class ConsulConfigurationProvider : ConfigurationProvider
	{
		private IEnumerable<ConsulAgentConfiguration> Config { get; }

		private bool ReloadOnChange { get; }

		public ConsulConfigurationProvider(IEnumerable<ConsulAgentConfiguration> config, bool reloadOnChange)
		{
			Config = config;
			ReloadOnChange = reloadOnChange;
		}

		public override void Load()
		{
			foreach (var conf in Config)
			{
				QueryConsulAsync(conf).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			if (ReloadOnChange) Task.Run(() => { ListenConsulKvChange(); });
		}

		private void ListenConsulKvChange()
		{
			Task.WaitAll(Config.Select(conf => Task.Run(async () =>
			{
				var failCount = 0;
				do
				{
					try
					{
						await QueryConsulAsync(conf, true);
						Console.WriteLine("Query comp;");
					}
					catch (TaskCanceledException)
					{
						failCount++;
					}
					catch (HttpRequestException)
					{
						failCount++;
					}
				} while (failCount <= conf.QueryOptions.CheckFailMaxTimes);
			})).ToArray());
		}

		private async Task QueryConsulAsync(ConsulAgentConfiguration conf, bool wait = false)
		{
			using (var client = new ConsulClient(options =>
			{
				options.WaitTime = conf.ClientConfiguration.WaitTime;
				options.Token = conf.ClientConfiguration.Token;
				options.Datacenter = conf.ClientConfiguration.Datacenter;
				options.Address = conf.ClientConfiguration.Address;
			}))
			{
				var result = await client.KV.List(conf.QueryOptions.Prefix, new QueryOptions
				{
					Token = conf.ClientConfiguration.Token,
					Datacenter = conf.ClientConfiguration.Datacenter,
					WaitIndex = conf.LastIndex,
					WaitTime = wait ? conf.QueryOptions.CheckChangeWaitTime : null
				});
				if (result.StatusCode == HttpStatusCode.OK && result.LastIndex > conf.LastIndex)
				{
					foreach (var item in result.Response)
					{
						if (conf.QueryOptions.TrimPrefix)
						{
							item.Key = item.Key.Substring(conf.QueryOptions.Prefix.Length, item.Key.Length - conf.QueryOptions.Prefix.Length);
						}
						Set(item.Key, item.Value != null || item.Value?.Length > 0 ? Encoding.UTF8.GetString(item.Value) : "");
					}
					conf.LastIndex = result.LastIndex;
					if (ReloadOnChange && wait)
						OnReload();
				}
			}
		}
	}
}
