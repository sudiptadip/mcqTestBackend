using mcqTestBackend.Model;
using mcqTestBackend.Service.IService;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mcqTestBackend.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateToken(ApplicationUser user, string role)
        {
            var claims = new[]
             {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim("role", user.Email),
                new Claim("email", role),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
