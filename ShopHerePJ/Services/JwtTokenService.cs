using Microsoft.IdentityModel.Tokens;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopHerePJ.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config) => _config = config;

        public string CreateToken(user u)
        {
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var expMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "120");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, u.userid.ToString()),
                new Claim(ClaimTypes.Email, u.email),
                new Claim(ClaimTypes.Role, u.role),
                new Claim("status", u.status)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
