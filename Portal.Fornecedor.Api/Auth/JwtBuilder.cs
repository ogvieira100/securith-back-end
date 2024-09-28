using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Portal.Fornecedor.Api.Models.User.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Portal.Fornecedor.Api.Auth
{
    public class JwtBuilder
    {
        private AppJwtSettings _appJwtSettings;
        public JwtBuilder(IOptions<AppJwtSettings> appJwtSettings)
        {
            _appJwtSettings = appJwtSettings.Value;   
        }

        public UserResponse BuildUserResponse()
        {
            var user = new UserResponse
            {
                AccessToken = BuildToken()
            };
            return user;
        }
        public string BuildToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appJwtSettings.SecretKey);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appJwtSettings.Issuer,
                Audience = _appJwtSettings.Audience,
                Expires = DateTime.UtcNow.AddHours(_appJwtSettings.Expiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            });
            return tokenHandler.WriteToken(token);
        }
    }
}
