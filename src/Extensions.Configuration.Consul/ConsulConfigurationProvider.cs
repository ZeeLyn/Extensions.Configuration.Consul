using System;
using System.Collections.Generic;
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
		private ConsulAgentConfiguration Config { get; }

		private bool ReloadOnChange { get; }

		public ConsulConfigurationProvider(ConsulAgentConfiguration config, bool reloadOnChange)
		{
			Config = config;
			ReloadOnChange = reloadOnChange;
		}

		public override void Load()
		{
			QueryConsulAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			if (ReloadOnChange) Task.Run(() => { ListenConsulKvChange(); });
		}

		private void ListenConsulKvChange()
		{
			var failCount = 0;
			do
			{
				try
				{
					QueryConsulAsync(true).ConfigureAwait(false).GetAwaiter().GetResult();
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
			} while (failCount <= Config.QueryOptions.CheckFailMaxTimes);
		}

		private async Task QueryConsulAsync(bool wait = false)
		{
			using (var client = new ConsulClient(options =>
			{
				options.WaitTime = Config.ClientConfiguration.WaitTime;
				options.Token = Config.ClientConfiguration.Token;
				options.Datacenter = Config.ClientConfiguration.Datacenter;
				options.Address = Config.ClientConfiguration.Address;
			}))
			{
				var result = await client.KV.List(Config.QueryOptions.Prefix, new QueryOptions
				{
					Token = Config.ClientConfiguration.Token,
					Datacenter = Config.ClientConfiguration.Datacenter,
					WaitIndex = Config.LastIndex,
					WaitTime = wait ? Config.QueryOptions.CheckChangeWaitTime : null
				});
				if (result.StatusCode == HttpStatusCode.NotFound)
					throw new KeyNotFoundException($"Cannot found key \"{ Config.QueryOptions.Prefix }\".");
				if (result.StatusCode == HttpStatusCode.OK && result.LastIndex > Config.LastIndex)
				{
					Data.Clear();
					foreach (var item in result.Response)
					{
						if (Config.QueryOptions.TrimPrefix)
						{
							item.Key = item.Key.Substring(Config.QueryOptions.Prefix.Length, item.Key.Length - Config.QueryOptions.Prefix.Length);
						}
						Set(item.Key, item.Value != null || item.Value?.Length > 0 ? Encoding.UTF8.GetString(item.Value) : "");
					}
					Config.LastIndex = result.LastIndex;
					if (ReloadOnChange && wait)
						OnReload();
				}
			}
		}
	}
}
