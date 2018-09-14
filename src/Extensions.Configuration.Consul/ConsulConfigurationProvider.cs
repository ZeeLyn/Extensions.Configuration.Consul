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
		private ConsulAgentConfiguration Configuration { get; }

		private bool ReloadOnChange { get; }

		private ulong LastIndex { get; set; }

		public ConsulConfigurationProvider(ConsulAgentConfiguration configuration, bool reloadOnChange)
		{
			Configuration = configuration;
			ReloadOnChange = reloadOnChange;
		}

		public override void Load()
		{
			QueryConsulAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			if (ReloadOnChange)
			{
				Task.Run(async () =>
				{
					var failCount = 0;
					do
					{
						try
						{
							await QueryConsulAsync(true);
							failCount = 0;
						}
						catch (TaskCanceledException)
						{
							failCount++;
							if (Configuration.QueryOptions.FailRetryInterval != null)
								await Task.Delay(Configuration.QueryOptions.FailRetryInterval.Value);
						}
						catch (ConsulRequestException)
						{
							failCount++;
							if (Configuration.QueryOptions.FailRetryInterval != null)
								await Task.Delay(Configuration.QueryOptions.FailRetryInterval.Value);
						}
						catch (HttpRequestException)
						{
							failCount++;
							if (Configuration.QueryOptions.FailRetryInterval != null)
								await Task.Delay(Configuration.QueryOptions.FailRetryInterval.Value);
						}
					} while (failCount <= Configuration.QueryOptions.ContinuousQueryFailures);
				});
			}
		}


		private async Task QueryConsulAsync(bool blocking = false)
		{
			using (var client = new ConsulClient(options =>
			{
				options.WaitTime = Configuration.ClientConfiguration.WaitTime;
				options.Token = Configuration.ClientConfiguration.Token;
				options.Datacenter = Configuration.ClientConfiguration.Datacenter;
				options.Address = Configuration.ClientConfiguration.Address;
			}))
			{
				var result = await client.KV.List(Configuration.QueryOptions.Prefix, new QueryOptions
				{
					Token = Configuration.ClientConfiguration.Token,
					Datacenter = Configuration.ClientConfiguration.Datacenter,
					WaitIndex = LastIndex,
					WaitTime = blocking ? Configuration.QueryOptions.BlockingQueryWait : null
				});
				if (result.LastIndex > LastIndex)
				{
					Data.Clear();
					if (result.Response != null)
					{
						foreach (var item in result.Response)
						{
							if (Configuration.QueryOptions.TrimPrefix)
							{
								item.Key = item.Key.Substring(Configuration.QueryOptions.Prefix.Length,
									item.Key.Length - Configuration.QueryOptions.Prefix.Length);
							}

							Set(item.Key,
								item.Value != null && item.Value?.Length > 0
									? Encoding.UTF8.GetString(item.Value)
									: "");
						}
					}
					LastIndex = result.LastIndex;
					if (ReloadOnChange && blocking)
						OnReload();
				}
			}
		}
	}
}
