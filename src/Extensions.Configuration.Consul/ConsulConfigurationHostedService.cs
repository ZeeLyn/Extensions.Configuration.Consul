using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Extensions.Configuration.Consul
{
    public class ConsulConfigurationHostedService : IHostedService
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private ulong LastIndex { get; set; }

        private HostedServiceOptions HostedServiceOptions { get; }

        private ILogger Logger { get; }

        public ConsulConfigurationHostedService(HostedServiceOptions hostedServiceOptions, ILogger<ConsulConfigurationHostedService> logger)
        {

            HostedServiceOptions = hostedServiceOptions;
            Logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
             {
                 do
                 {
                     try
                     {
                         await QueryConsulAsync();
                     }
                     catch (Exception e)
                     {
                         Logger.LogError($"{e.Message}");
                     }

                 } while (!CancellationTokenSource.IsCancellationRequested);
             }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        private async Task QueryConsulAsync()
        {
            using (var client = new ConsulClient(options =>
            {
                options.WaitTime = ObserverManager.Configuration.ClientConfiguration.WaitTime;
                options.Token = ObserverManager.Configuration.ClientConfiguration.Token;
                options.Datacenter = ObserverManager.Configuration.ClientConfiguration.Datacenter;
                options.Address = ObserverManager.Configuration.ClientConfiguration.Address;
            }))
            {
                var result = await client.KV.List(ObserverManager.Configuration.QueryOptions.Folder, new QueryOptions
                {
                    Token = ObserverManager.Configuration.ClientConfiguration.Token,
                    Datacenter = ObserverManager.Configuration.ClientConfiguration.Datacenter,
                    WaitIndex = LastIndex,
                    WaitTime = HostedServiceOptions.BlockingQueryWait
                }, CancellationTokenSource.Token);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    return;
                if (result.LastIndex > LastIndex)
                {
                    LastIndex = result.LastIndex;
                    ObserverManager.Notify(result.Response.ToList(), Logger);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();
            await Task.CompletedTask;
        }
    }
}
