using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using System;
using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Consul
{
	internal class ConsulConfigurationProvider : ConfigurationProvider, IDisposable
	{
		private ConsulAgentConfiguration Configuration { get; }

		private bool ReloadOnChange { get; }

		private ulong LastIndex { get; set; }

		private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

		public ConsulConfigurationProvider(ConsulAgentConfiguration configuration, bool reloadOnChange)
		{
			Configuration = configuration;
			ReloadOnChange = reloadOnChange;
		}


		public static void Shutdown()
		{
			CancellationTokenSource.Cancel();
		}

		public override void Load()
		{
			QueryConsulAsync(CancellationTokenSource.Token).GetAwaiter().GetResult();
			if (ReloadOnChange)
			{
				Task.Factory.StartNew(async () =>
				{
					var failCount = 0;
					do
					{
						try
						{
							await QueryConsulAsync(CancellationTokenSource.Token, true);
							failCount = 0;
						}
						catch (TaskCanceledException)
						{
							failCount++;
							if (Configuration.QueryOptions.FailRetryInterval != null)
								await Task.Delay(Configuration.QueryOptions.FailRetryInterval.Value, CancellationTokenSource.Token);
						}
						catch (ConsulRequestException)
						{
							failCount++;
							if (Configuration.QueryOptions.FailRetryInterval != null)
								await Task.Delay(Configuration.QueryOptions.FailRetryInterval.Value, CancellationTokenSource.Token);
						}
						catch (HttpRequestException)
						{
							failCount++;
							if (Configuration.QueryOptions.FailRetryInterval != null)
								await Task.Delay(Configuration.QueryOptions.FailRetryInterval.Value, CancellationTokenSource.Token);
						}
					} while (!CancellationTokenSource.IsCancellationRequested && failCount <= Configuration.QueryOptions.ContinuousQueryFailures);
				}, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}
		}


		private async Task QueryConsulAsync(CancellationToken cancellationToken, bool blocking = false)
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
				}, cancellationToken);
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

							if (!string.IsNullOrWhiteSpace(item.Key))
							{
								Set(item.Key, item.Value != null && item.Value?.Length > 0 ? Encoding.UTF8.GetString(item.Value) : "");
							}
						}
					}
					LastIndex = result.LastIndex;
					if (ReloadOnChange && blocking)
						OnReload();
				}
			}
		}

		public void Dispose()
		{
			CancellationTokenSource.Cancel();
		}
	}
}
