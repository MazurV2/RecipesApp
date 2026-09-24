using RecipesApi.DTOs.Auth;
using System.Net;
using System.Net.Http.Json;

namespace RecipesApi.Tests.Integration_Tests
{
    public class AuthControllerTests : BaseIntegrationTests, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        public AuthControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnej rejestracji użytkownika
        [Fact]
        public async Task Register_ValidData_ShouldReturnSuccess()
        {
            // Arrange
            var registerDTO = new RegisterDTO
            {
                Username = "test",
                Email = "test@example.com",
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync("/api/Auth/register", registerDTO);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź, czy użytkownik został dodany do bazy danych
            var userInDb = await GetUserIfExistsAsync(registerDTO.Username);

            Assert.NotNull(userInDb);
            Assert.Equal(registerDTO.Email, userInDb.Email);
        }

        // Test rejestracji z błędnymi danymi
        [Theory]
        [InlineData("", "test@example.com", "TestPassword123!", "TestPassword123!")] // Błędna nazwa
        [InlineData("test", "invalid-email", "TestPassword123!", "TestPassword123!")] // Błędny email
        [InlineData("test", "test@example.com", "invalid-password", "invalid-password")] // Błędne hasło
        [InlineData("test", "test@example.com", "TestPassword123!", "invalid-passwordConfirmation")] // Błędne potwierdzenie hasła
        public async Task Register_InvalidData_ShouldReturnBadRequest(string username, string email, string password, string passwordConfirmation)
        {
            // Arrange
            var registerDTO = new RegisterDTO
            {
                Username = username,
                Email = email,
                Password = password,
                PasswordConfirmation = passwordConfirmation
            };

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync("/api/Auth/register", registerDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test rejestracji z istniejącym użytkownikiem
        [Fact]
        public async Task Register_ExistingUser_ShouldReturnBadRequest()
        {
            // Arrange
            string password = "TestPassword123!";

            var user = await AddUserToDatabaseAsync(password: password);

            var registerDTO = new RegisterDTO
            {
                Username = user.Username,
                Email = user.Email,
                Password = password,
                PasswordConfirmation = password
            };

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync("/api/Auth/register", registerDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test poprawnego logowania użytkownika
        [Theory]
        [InlineData(true)] // Logowanie za pomocą nazwy użytkownika
        [InlineData(false)] // Logowanie za pomocą adresu email
        public async Task Login_ValidData_ShouldReturnSuccess(bool useUsername)
        {
            // Arrange
            var password = "TestPassword123!";
            var user = await AddUserToDatabaseAsync(password: password);

            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = useUsername ? user.Username : user.Email,
                Password = password,
            };

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync("/api/Auth/login", loginDTO);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Test logowania z błędnymi danymi
        [Theory]
        [InlineData(true, "NonexistingPassword123!")] // Logowanie za pomocą nazwy użytkownika z błędnym hasłem
        [InlineData(false, "NonexistingPassword123!")] // Logowanie za pomocą adresu email z błędnym hasłem
        [InlineData(true, "TestPassword123!", "NonexistingUser")] // Logowanie na nieistniejącego użytkownika
        public async Task Login_InvalidData_ShouldReturnUnauthorized(bool useUsername, string password, string? nonexistentUsername = null)
        {
            // Arrange
            var userPassword = "TestPassword123!";
            var user = await AddUserToDatabaseAsync(password: userPassword);

            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = nonexistentUsername ?? (useUsername ? user.Username : user.Email),
                Password = password,
            };

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync("/api/Auth/login", loginDTO);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
