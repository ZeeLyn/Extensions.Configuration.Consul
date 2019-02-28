using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Extensions.Configuration.Consul.UI.Controllers
{
    [Route("api/key")]
    [Authorize]
    public class KeyController : Controller
    {
        private IConsulClient Client { get; }

        public KeyController(IConsulClient client)
        {
            Client = client;
        }

        [HttpGet("nodes")]
        public async Task<IActionResult> Nodes()
        {
            var result = await Client.KV.List(ObserverManager.Configuration.QueryOptions.Folder, new QueryOptions
            {
                Token = ObserverManager.Configuration.ClientConfiguration.Token,
                Datacenter = ObserverManager.Configuration.ClientConfiguration.Datacenter
            });
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest();
            return Ok(FormatKey.FormatFolder(result.Response.ToDictionary(key => key.Key,
                value => ByteToString(value.Value))));

        }

        [HttpPut("put")]
        public async Task<IActionResult> Put([FromBody]KeyValue_Value keyValue)
        {
            if (keyValue.Key.EndsWith(":"))
                return BadRequest("Key is not allowed to end with ':'");

            var result = await Client.KV.Put(new KVPair(keyValue.Key) { Value = StringToByte(keyValue.Value) });
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest("error");
            return Ok(true);

        }

        [HttpDelete("delete/{delChildren:bool}")]
        public async Task<IActionResult> Delete([FromBody]KeyValue_Key key, bool delChildren)
        {
            var result = delChildren ? await Client.KV.DeleteTree(key.Key) : await Client.KV.Delete(key.Key);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest("error");
            return Ok(true);
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
