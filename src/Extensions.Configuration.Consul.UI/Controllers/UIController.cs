using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Extensions.Configuration.Consul.UI.Controllers
{
    [Route("ui")]
    public class UIController : Controller
    {
        [HttpGet("index")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
