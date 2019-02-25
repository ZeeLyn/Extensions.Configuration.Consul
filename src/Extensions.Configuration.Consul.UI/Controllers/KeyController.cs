using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Mvc;

namespace Extensions.Configuration.Consul.UI.Controllers
{
    [Route("key")]
    public class KeyController : Controller
    {
        [HttpGet("nodes")]
        public async Task<IActionResult> Nodes()
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
                    Datacenter = ObserverManager.Configuration.ClientConfiguration.Datacenter
                });
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    return BadRequest();
                return Ok(FormatKey.FormatFolder(result.Response.ToDictionary(key => key.Key,
                    value => ByteToString(value.Value))));
            }
        }

        [HttpPut("put")]
        public async Task<IActionResult> Edit([FromBody]KeyValue keyValue)
        {
            using (var client = new ConsulClient(options =>
            {
                options.WaitTime = ObserverManager.Configuration.ClientConfiguration.WaitTime;
                options.Token = ObserverManager.Configuration.ClientConfiguration.Token;
                options.Datacenter = ObserverManager.Configuration.ClientConfiguration.Datacenter;
                options.Address = ObserverManager.Configuration.ClientConfiguration.Address;
            }))
            {
                var result = await client.KV.Put(new KVPair(keyValue.Key) { Value = StringToByte(keyValue.Value) });
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    return BadRequest();
                return Ok();
            }
        }

        private string ByteToString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return "";
            return Encoding.UTF8.GetString(bytes);
        }

        private byte[] StringToByte(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return Encoding.UTF8.GetBytes(value);
        }
    }
}
