using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Extensions.Configuration.Consul;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private Configs Config;
        private IConfiguration _conf;
        private LibClass lib;
        private SingleClass single;
        public ValuesController(IOptionsSnapshot<Configs> config, IConfiguration conf, LibClass _lib, SingleClass _single)
        {
            Config = config.Value;
            _conf = conf;
            lib = _lib;
            single = _single;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            return Ok(new { option = Config, configuration = _conf.GetSection("Db").Get<Configs>(), lib = lib.Get(), single = single.Get(), cfg = new ConsulAgentConfiguration() });
        }


    }
}
