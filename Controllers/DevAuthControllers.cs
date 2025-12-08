using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PcOne.Api.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevAuthController : ControllerBase
    {
        private readonly IConfiguration _cfg;
        private readonly IWebHostEnvironment _env;

        public DevAuthController(IConfiguration cfg, IWebHostEnvironment env)
        {
            _cfg = cfg;
            _env = env;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // Solo disponible en Development
            if (!_env.IsDevelopment())
                return NotFound();

            // Credenciales temporales para desarrollo
            if (req.Username == "Bryan" && req.Password == "123")
            {
                var jwtKey = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt Key no configurada");
                var jwtIssuer = _cfg["Jwt:Issuer"];
                var jwtAudience = _cfg["Jwt:Audience"];
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, req.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { accessToken = tokenString });
            }

            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        public class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
