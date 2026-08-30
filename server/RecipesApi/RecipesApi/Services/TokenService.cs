using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RecipesApi.Entities;
using RecipesApi.Services.Interfaces;
using RecipesApi.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecipesApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public (string, DateTime) GenerateJwtToken(User user)
        {
            // Określ informacje zawarte w tokenie
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Pobierz sekret i stwórz klucz szyfrujący
            var secretKey = _jwtSettings.Secret;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            // Wygeneruj token (JWT)
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiryDate,
                signingCredentials: credentials
                );

            var writtenToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (writtenToken, expiryDate);
        }
    }
}
