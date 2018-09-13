using System;

namespace Extensions.Configuration.Consul
{
	public class ConsulQueryOptions
	{
		/// <summary>
		/// The prefix string of consul key
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Remove prefix string of consul key
		/// </summary>
		public bool TrimPrefix { get; set; } = true;

		/// <summary>
		/// Wait time of Long polling,the defautl is 2 minutes.
		/// </summary>
		public TimeSpan? CheckChangeWaitTime { get; set; } = TimeSpan.FromMinutes(1);

		/// <summary>
		/// The number of failed attempts, the default is 20
		/// </summary>
		public int CheckFailMaxTimes { get; set; } = 20;
	}
}
