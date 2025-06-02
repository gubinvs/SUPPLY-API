using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


/// <summary>
/// Класс решает задачу по генерации токенов
/// </summary>
public class TokenService
{
    private const string SecretKey = "YourSecureKeyHereMustBeLongEnough"; // Совпадает с ключом в Program.cs
    private const int TokenExpiryMinutes = 14400; // Срок действия токена 24 часа

    public string GenerateToken(string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);
        if (key.Length < 32) // 32 байта = 256 бит
        {
            throw new ArgumentException("Key must be at least 256 bits long.");
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Уникальный идентификатор
        }),
            Expires = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}
