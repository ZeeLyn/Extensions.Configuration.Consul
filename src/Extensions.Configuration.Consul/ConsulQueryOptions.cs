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
		/// Remove prefix string of consul key when binding
		/// </summary>
		public bool TrimPrefix { get; set; } = false;

		/// <summary>
		/// Wait time of long polling,the defautl is 2 minutes
		/// </summary>
		public TimeSpan? BlockingQueryWait { get; set; } = TimeSpan.FromMinutes(2);

		/// <summary>
		/// Continuous query failures, the default is 20
		/// </summary>
		public int ContinuousQueryFailures { get; set; } = 20;

		/// <summary>
		/// Failure retry interval,the default is 2 minutes
		/// </summary>
		public TimeSpan? FailRetryInterval { get; set; } = TimeSpan.FromMinutes(2);
	}
}
