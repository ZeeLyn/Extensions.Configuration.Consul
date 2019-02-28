using System;
using Consul;

namespace Extensions.Configuration.Consul
{
    public class ConsulAgentConfiguration
    {
        public ConsulClientConfiguration ClientConfiguration { get; set; } = new ConsulClientConfiguration
        {
            Address = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("consul-configuration-addr"))
                ? null
                : new Uri(Environment.GetEnvironmentVariable("consul-configuration-addr") ?? "http://127.0.0.1:8500"),
            Token = Environment.GetEnvironmentVariable("consul-configuration-token") ?? "",
            Datacenter = Environment.GetEnvironmentVariable("consul-configuration-dc") ?? "dc1",
            WaitTime = TimeSpan.FromSeconds(10)
        };

        public ConsulQueryOptions QueryOptions { get; set; } = new ConsulQueryOptions();
    }
}
