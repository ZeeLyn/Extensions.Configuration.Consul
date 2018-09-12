using System;
using System.Collections.Generic;
using System.Text;
using Consul;

namespace Extensions.Configuration.Consul
{
	public class ConsulAgentConfiguration
	{
		public ConsulClientConfiguration ClientConfiguration { get; set; }

		public ConsulQueryOptions QueryOptions { get; set; }
	}
}
