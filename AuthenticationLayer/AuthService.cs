
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationLayer
{
    public class AuthService
    {
        private readonly string _secretKey;

        public AuthService(string secretKey)
        {
            _secretKey = secretKey;
        }



        // this is central place configruation for the authenticaition of the token based on the defined rules that can be used within and outside this library
        public TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)), // Getting symetric key from the secret key bytes.
                ValidateIssuer = false,     // Not validating token issuer
                ValidateAudience = false,
                ValidateLifetime = true     // Validating token expiry 
            };
        }

        // This function will be called to generate the tokens by passing user unique identifer preferably Id
        public string GenerateToken(string userData)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            // Getting bytes data of the the security key and then creating Symetric security key from that.
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDesc = new SecurityTokenDescriptor
            {
                Issuer = "EmpowerID",
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userData)
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = signingCredentials
            };

            var token = handler.CreateToken(tokenDesc);
            return handler.WriteToken(token);

            
        }

        // This function will be called to validate the tokens and returns the claims if the token is validated otherwise null.
        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken = null;
                // validating token...
                var userData = tokenHandler.ValidateToken(token, GetTokenValidationParameters(), out validatedToken);
                return userData;
            }
            catch
            {
                // token validation failed
                return null;
            }

        }


    }
}
