using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.API.DTO;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace Auth.API.Services
{
    public class TokenGenerator
    {
        private readonly TokenOptions _tokenOptions;

        public TokenGenerator(IOptions<TokenOptions> options)
        {
            _tokenOptions = options.Value;
        }

        public string Generate(UserResponseDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(_tokenOptions.ExpiresDay),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.Secret)),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private ClaimsIdentity AddClaims(UserResponseDto user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.Role, user.Role!));
            return claims;
        }
    };
}

