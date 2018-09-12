using System;

namespace Extensions.Configuration.Consul
{
	public class ConsulQueryOptions
	{
		/// <summary>
		/// Filter the key of prefix
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Remove the key of prefix string
		/// </summary>
		public bool TrimPrefix { get; set; } = true;

		/// <summary>
		/// Long polling wait time
		/// </summary>
		public TimeSpan? CheckChangeWaitTime { get; set; } = TimeSpan.FromMinutes(2);

		public int CheckFailMaxTimes { get; set; } = 10;
	}
}
