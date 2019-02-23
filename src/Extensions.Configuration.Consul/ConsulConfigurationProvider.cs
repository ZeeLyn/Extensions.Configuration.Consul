using System.Text;
using System.Threading.Tasks;
using Consul;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Extensions.Configuration.Consul
{
    internal class ConsulConfigurationProvider : ConfigurationProvider, IObserver
    {
        private ConsulAgentConfiguration Configuration { get; }


        public ConsulConfigurationProvider(ConsulAgentConfiguration configuration)
        {
            Configuration = configuration;
        }


        public override void Load()
        {
            //QueryConsulAsync().GetAwaiter().GetResult();
        }


        private async Task QueryConsulAsync()
        {
            using (var client = new ConsulClient(options =>
            {
                options.WaitTime = Configuration.ClientConfiguration.WaitTime;
                options.Token = Configuration.ClientConfiguration.Token;
                options.Datacenter = Configuration.ClientConfiguration.Datacenter;
                options.Address = Configuration.ClientConfiguration.Address;
            }))
            {
                var result = await client.KV.List(Configuration.QueryOptions.Folder, new QueryOptions
                {
                    Token = Configuration.ClientConfiguration.Token,
                    Datacenter = Configuration.ClientConfiguration.Datacenter
                });

                if (result.Response == null || !result.Response.Any())
                    return;

                foreach (var item in result.Response)
                {
                    item.Key = item.Key.TrimFolderPrefix(Configuration.QueryOptions.Folder);
                    if (string.IsNullOrWhiteSpace(item.Key))
                        return;
                    Set(item.Key, ReadValue(item.Value));
                }
            }
        }


        private string ReadValue(byte[] bytes)
        {
            return bytes != null && bytes.Length > 0
                ? Encoding.UTF8.GetString(bytes)
                : "";
        }

        public void OnChange(List<KVPair> kVs, ILogger logger)
        {
            if (kVs == null || !kVs.Any())
            {
                Data.Clear();
                OnReload();
                return;
            }

            var deleted = Data.Where(p => kVs.All(c =>
                p.Key != c.Key.TrimFolderPrefix(Configuration.QueryOptions.Folder))).ToList();

            foreach (var del in deleted)
            {
                logger.LogTrace($"Remove key [{del.Key}]");
                Data.Remove(del.Key);
            }

            foreach (var item in kVs)
            {
                item.Key = item.Key.TrimFolderPrefix(Configuration.QueryOptions.Folder);
                if (string.IsNullOrWhiteSpace(item.Key))
                    continue;
                var newValue = ReadValue(item.Value);
                if (Data.TryGetValue(item.Key, out var oldValue))
                {
                    if (oldValue == newValue)
                        continue;

                    Set(item.Key, newValue);
                    logger.LogTrace($"The value of key [{item.Key}] is changed from [{oldValue}] to [{newValue}]");
                }
                else
                {
                    Set(item.Key, newValue);
                    logger.LogTrace($"Added key [{item.Key}][{newValue}]");
                }
                OnReload();
            }
        }
    }
}
