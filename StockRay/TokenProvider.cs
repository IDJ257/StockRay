using Microsoft.IdentityModel.Tokens;
using StockRay.Models;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;


namespace StockRay
{
    public class TokenProvider
    {
        private readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public string Create(User user)
        {
            string secretKey = _configuration["Jwt:Secret"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                     new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())

                    ]),
                Expires = DateTime.UtcNow.AddMinutes(45),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };


            var handler = new JsonWebTokenHandler();

            string token = handler.CreateToken(tokenDescriptor);

            return token;
        }
    }
}
