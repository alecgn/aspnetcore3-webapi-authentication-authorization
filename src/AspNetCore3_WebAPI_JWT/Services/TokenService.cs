
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AspNetCore3_WebAPI_JWT.Models;
using AspNetCore3_WebAPI_JWT.CustomExceptions;
using Microsoft.Extensions.Configuration;
using AspNetCore3_WebAPI_JWT.Enums;
using CryptHash.Net.Encoding;
using System.Security.Cryptography;
using CryptHash.Net.Util;

namespace AspNetCore3_WebAPI_JWT.Services 
{
    public class TokenService 
    {
        private IConfiguration Configuration;

        public TokenService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public string GenerateToken(User user)
        {
            var securityKeyStr = _configuration.GetValue<string>("AppSettings:SecurityKey");

            if (string.IsNullOrWhiteSpace(securityKeyStr))
                throw new ConfigurationErrorsException("SecurityKey cannot be null, empty or whitespace, check configuration file.");

            var securityKeyBytes = Encoding.UTF8.GetBytes(securityKeyStr);
            var tokenExpirationInMinutes = _configuration.GetValue<int>("AppSettings:TokenExpirationInMinutes");
            tokenExpirationInMinutes = (tokenExpirationInMinutes <= 0 ? 15 : tokenExpirationInMinutes);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.Name, user.Username),
                     new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
