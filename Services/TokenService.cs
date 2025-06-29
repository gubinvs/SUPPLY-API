using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace SUPPLY_API
{
    /// <summary>
    /// Класс решает задачу по генерации токенов
    /// </summary>
    public class TokenService
    {
        private readonly string _secretKey;
        private const int TokenExpiryMinutes = 1440;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _secretKey = jwtSettings.Value.SecretKey;

            if (string.IsNullOrWhiteSpace(_secretKey) || Encoding.UTF8.GetByteCount(_secretKey) < 32)
                throw new ArgumentException("JwtSettings:SecretKey must be configured and at least 256 bits long.");
        }

        public string GenerateToken(string email, string role)
        {
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                Expires = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}