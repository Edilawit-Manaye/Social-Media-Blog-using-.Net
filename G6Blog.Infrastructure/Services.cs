using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using G6Blog.Application.Services; // Ensure we use the interfaces from Application
using System.Linq;

namespace G6Blog.Infrastructure.Services
{
    // Removing Interface definitions from here

    public class JwtService : IJwtService
    {
        private readonly string _secret;
        private readonly string _issuer;

        public JwtService(string secret, string issuer)
        {
            _secret = secret;
            _issuer = issuer;
        }

        public string GenerateToken(string userId, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("role", role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_issuer,
                _issuer,
                claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string ValidateToken(string token)
        {
             var tokenHandler = new JwtSecurityTokenHandler();
             var key = Encoding.UTF8.GetBytes(_secret);
             try {
                 tokenHandler.ValidateToken(token, new TokenValidationParameters {
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(key),
                     ValidateIssuer = true,
                     ValidIssuer = _issuer,
                     ValidateAudience = true,
                     ValidAudience = _issuer,
                     ClockSkew = TimeSpan.Zero
                 }, out SecurityToken validatedToken);
                 
                 var jwtToken = (JwtSecurityToken)validatedToken;
                 return jwtToken.Claims.First(x => x.Type == "sub").Value; // return user id
             } catch {
                 return null;
             }
        }
    }

    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
