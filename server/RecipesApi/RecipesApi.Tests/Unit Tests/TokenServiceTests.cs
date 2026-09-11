using Microsoft.Extensions.Options;
using RecipesApi.Entities;
using RecipesApi.Services;
using RecipesApi.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecipesApi.Tests
{
    public class TokenServiceTests
    {
        private readonly JwtSettings _jwtSettings;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                Secret = "TestSecretKeyTestSecretKeyTestSecretKeyTestSecretKey",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryInMinutes = 5
            };

            var options = Options.Create(_jwtSettings);

            _tokenService = new TokenService(options);
        }

        [Fact]
        public void GenerateJwtToken_ShouldReturnValidTokenAndExpiryDate()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "test",
                Email = "test@example.com",
            };

            // Act
            var (token, expiryDate) = _tokenService.GenerateJwtToken(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Equal(2, token.Count(c => c == '.'));
            Assert.True(expiryDate > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateJwtToken_ShouldContainCorrectClaims()
        {
            // Arrange
            var id = 1;
            var username = "test";
            var email = "test@example.com";

            var user = new User
            {
                Id = id,
                Username = username,
                Email = email
            };
            
            // Act
            var (token, expiryDate) = _tokenService.GenerateJwtToken(user);

            // Przygotuj wbudowaną klasę do walidacji tokenów JWT
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Pobierz informacje z tokenu
            var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            // Assert

            // Sprawdź, czy token zawiera poprawne informacje
            Assert.Equal(id.ToString(), nameIdentifierClaim.Value);
            Assert.Equal(username, nameClaim.Value);
            Assert.Equal(email, emailClaim.Value);

            Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        }
    }
}
