using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Extensions.Configuration.Consul.UI.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        [HttpGet("check")]
        public IActionResult Check()
        {
            return Ok(System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".secure")) ? 1 : 0);
        }

        [HttpPost("set")]
        public IActionResult SetPassword([FromBody]SetPassword body)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(e => e.ErrorMessage).LastOrDefault();
                return BadRequest(firstError);
            }

            if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".secure")))
                return BadRequest();
            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), ".secure"), SHA512(body.Password));

            return Ok(GenerateToken());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]PasswordBase body)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(e => e.ErrorMessage).LastOrDefault();
                return BadRequest(firstError);
            }

            var pwd = await System.IO.File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(),
                ".secure"));
            if (pwd == SHA512(body.Password))
                return Ok(GenerateToken());
            return BadRequest("The password is incorrect");
        }

        private string GenerateToken()
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var now = DateTime.UtcNow;
            var jwtHeader = new JwtHeader(new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CC9800B9-DFEF-4644-B862-FB630DFB880E")),
                SecurityAlgorithms.HmacSha512Signature));
            var jwtPayload = new JwtPayload(
                "consul configuration center",
                "consul configuration center user",
                claims,
                now,
                now.Add(TimeSpan.FromDays(1)),
                now);
            var jwt = new JwtSecurityToken(jwtHeader, jwtPayload);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private string SHA512(string sourceText)
        {
            using (var sha512 = new SHA512Managed())
            {
                var tmpByte = Encoding.UTF8.GetBytes(sourceText);
                var bytes = sha512.ComputeHash(tmpByte);
                sha512.Clear();
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }



    public class PasswordBase
    {
        [Required(ErrorMessage = "Please enter your password!")]
        [MinLength(6, ErrorMessage = "Need at least 6 characters.")]
        [MaxLength(18, ErrorMessage = "Up to 18 characters long.")]
        public string Password { get; set; }
    }

    public class SetPassword : PasswordBase
    {
        [Required(ErrorMessage = "Please re-enter your password!")]
        [MinLength(6, ErrorMessage = "Need at least 6 characters.")]
        [MaxLength(18, ErrorMessage = "Up to 18 characters long.")]
        [Compare("Password", ErrorMessage = "Inconsistent password entered twice")]
        public string ReEnter { get; set; }
    }
}
