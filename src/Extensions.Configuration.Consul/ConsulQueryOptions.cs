using System;

namespace Extensions.Configuration.Consul
{
    public class ConsulQueryOptions
    {
        /// <summary>
        /// The prefix string of consul key
        /// </summary>
        public string Folder { get; set; }

    }

    public class HostedServiceOptions
    {
        public TimeSpan BlockingQueryWait { get; set; } = TimeSpan.FromMinutes(3);
    }
}
