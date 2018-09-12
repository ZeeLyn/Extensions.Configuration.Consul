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

		private Dictionary<string, ulong> ConsulMaxIndex { get; }

		private Task ListenConsulTask { get; }

		public ConsulConfigurationProvider(IEnumerable<ConsulAgentConfiguration> config, bool reloadOnChange)
		{
			Config = config;
			ConsulMaxIndex = Config.ToDictionary(key => key.ClientConfiguration.Address.ToString() + ":" + key.QueryOptions.Prefix, value => 0ul);
			ReloadOnChange = reloadOnChange;
			if (reloadOnChange)
				ListenConsulTask = new Task(ListenConsulKvChange);
		}

		public override void Load()
		{
			foreach (var conf in Config)
			{
				QueryConsulAsync(conf).ConfigureAwait(false).GetAwaiter().GetResult();
			}

			if (ReloadOnChange && ListenConsulTask != null && ListenConsulTask.Status == TaskStatus.Created)
				ListenConsulTask.Start();
		}

		private void ListenConsulKvChange()
		{
			Parallel.ForEach(Config, async conf =>
			{
				var failCount = 0;
				do
				{
					try
					{
						await QueryConsulAsync(conf, true);
					}
					catch (TaskCanceledException)
					{
						failCount++;
					}
					catch (HttpRequestException)
					{
						failCount++;
					}
				} while (failCount > conf.QueryOptions.CheckFailMaxTimes);
			});
		}

		private async Task QueryConsulAsync(ConsulAgentConfiguration conf, bool wait = false)
		{
			var lastIndex = ConsulMaxIndex[conf.ClientConfiguration.Address + ":" + conf.QueryOptions.Prefix];
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
					WaitIndex = lastIndex,
					WaitTime = wait ? conf.QueryOptions.CheckChangeWaitTime : default(TimeSpan)
				});
				if (result.StatusCode == HttpStatusCode.OK && result.LastIndex > lastIndex)
				{
					foreach (var item in result.Response)
					{
						if (conf.QueryOptions.TrimPrefix)
						{
							item.Key = item.Key.Substring(conf.QueryOptions.Prefix.Length, item.Key.Length - conf.QueryOptions.Prefix.Length);
						}
						Set(item.Key, item.Value != null || item.Value?.Length > 0 ? Encoding.UTF8.GetString(item.Value) : "");
					}
					ConsulMaxIndex[conf.ClientConfiguration.Address + ":" + conf.QueryOptions.Prefix] =
						result.LastIndex;
					if (wait)
						OnReload();
				}
			}
		}
	}
}
