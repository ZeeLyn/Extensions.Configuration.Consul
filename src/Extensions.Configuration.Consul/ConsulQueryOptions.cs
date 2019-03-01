using System;

namespace Extensions.Configuration.Consul
{
    public class ConsulQueryOptions
    {
        public ConsulQueryOptions()
        {
            var folder = Environment.GetEnvironmentVariable("consul-configuration-folder");
            if (folder != null && !folder.EndsWith("/"))
                throw new ArgumentException("Folder must end with \"/\".");
            Folder = Environment.GetEnvironmentVariable("consul-configuration-folder") ?? "";
        }

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
