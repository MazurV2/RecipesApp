using RecipesApi.DTOs.User;
using System.Net;
using System.Net.Http.Json;

namespace RecipesApi.Tests.Integration_Tests
{
    public class UserControllerTests : BaseIntegrationTests, IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private string apiURL = "/api/User";

        public UserControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test pobierania użytkowników
        [Fact]
        public async Task GetUsers_ShouldReturnOK()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            // Dodaj przykładowego użytkownika do bazy danych
            await AddUserToDatabaseAsync(username: "Test User 1");
            await AddUserToDatabaseAsync(username: "Test User 2");
            await AddUserToDatabaseAsync(username: "Test User 3");

            // Act
            var response = await client.GetAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = await response.Content.ReadFromJsonAsync<IEnumerable<UserDTO>>();
            Assert.NotNull(results);
            Assert.True(results.Count() >= 3);
        }

        // Test nieautoryzowanego pobierania użytkowników
        [Fact]
        public async Task GetUsers_ShouldReturnUnauthorized()
        {
            // Arrange
            
            // Act
            var response = await _unauthorizedClient.GetAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test pobierania użytkownika po ID
        [Fact]
        public async Task GetUser_ShouldReturnOK()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            
            // Act
            var response = await client.GetAsync($"{apiURL}/{userId}");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        // Test pobierania nieistniejącego użytkownika po ID
        [Fact]
        public async Task GetUser_Nonexistent_ShouldReturnNotFound()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/999";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test nieautoryzowanego pobierania użytkownika po ID
        [Fact]
        public async Task GetUser_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = await AddUserToDatabaseAsync();
            var url = $"{apiURL}/{user.Id}";

            // Act
            var response = await _unauthorizedClient.GetAsync(url);
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego tworzenia użytkownika
        [Fact]
        public async Task CreateUser_ValidData_ShouldReturnCreated()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();
            var createUserDTO = GetCreateUserDTOAsync();

            // Act
            var response = await client.PostAsJsonAsync(apiURL, createUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Sprawdź, czy użytkownik został dodany do bazy danych
            var userInDb = await GetUserIfExistsAsync(createUserDTO.Username);
            Assert.NotNull(userInDb);
        }

        // Test tworzenia użytkownika z niepoprawnymi danymi
        [Theory]
        [InlineData(null, "test@example.com", "TestPassword123!")] // Brak nazwy
        [InlineData("Test", "invalid-email", "TestPassword123!")] // Błędny email
        [InlineData("Test", "test@example.com", "invalid-password")] // Błędne hasło
        public async Task CreateUser_InvalidData_ShouldReturnBadRequest(string? username, string email, string password)
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();
            var createUserDTO = GetCreateUserDTOAsync(username, email, password);

            // Act
            var response = await client.PostAsJsonAsync(apiURL, createUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Sprawdź, czy użytkownik nie został dodany do bazy danych
            var userInDb = await GetUserIfExistsAsync(createUserDTO.Username);
            Assert.Null(userInDb);
        }

        // Test tworzenia użytkownika z istniejącą nazwą lub emailem
        [Fact]
        public async Task CreateUser_DuplicateData_ShouldReturnConflict()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();
            var existingUser = await AddUserToDatabaseAsync();
            
            var createUserDTO1 = GetCreateUserDTOAsync(username: existingUser.Username);
            var createUserDTO2 = GetCreateUserDTOAsync(email: existingUser.Email);

            // Act
            var response1 = await client.PostAsJsonAsync(apiURL, createUserDTO1);
            var response2 = await client.PostAsJsonAsync(apiURL, createUserDTO2);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
        }

        // Test nieautoryzowanego tworzenia użytkownika
        [Fact]
        public async Task CreateUser_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            var createUserDTO = GetCreateUserDTOAsync();

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync(apiURL, createUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnej aktualizacji użytkownika
        [Fact]
        public async Task UpdateUser_ValidData_ShouldReturnOK()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/{userId}";
            
            var updateUserDTO = GetUpdateUserDTOAsync();

            // Act
            var response = await client.PutAsJsonAsync(url, updateUserDTO);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Sprawdź, czy użytkownik został zaktualizowany w bazie danych
            var userInDb = await GetUserIfExistsAsync(updateUserDTO.Username);
            Assert.NotNull(userInDb);
            Assert.Equal(updateUserDTO.Email, userInDb.Email);
        }

        // Test aktualizacji użytkownika z niepoprawnymi danymi
        [Theory]
        [InlineData(null, "updatedTest@example.com", "UpdatedTestPassword123!")]
        [InlineData("Updated Test", "invalid-email", "UpdatedTestPassword123!")]
        [InlineData("Updated Test", "Updated@example.com", "invalid-password")]
        public async Task UpdateUser_InvalidData_ShouldReturnBadRequest(string? username, string email, string password)
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/{userId}";
            
            var updateUserDTO = GetUpdateUserDTOAsync(username, email, password);

            // Act
            var response = await client.PutAsJsonAsync(url, updateUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test aktualizacji użytkownika z istniejącą nazwą lub emailem
        [Fact]
        public async Task UpdateUser_DuplicateData_ShouldReturnConflict()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var existingUser = await AddUserToDatabaseAsync();
            var url = $"{apiURL}/{userId}";
            
            var updateUserDTO1 = GetUpdateUserDTOAsync(username: existingUser.Username);
            var updateUserDTO2 = GetUpdateUserDTOAsync(email: existingUser.Email);

            // Act
            var response1 = await client.PutAsJsonAsync(url, updateUserDTO1);
            var response2 = await client.PutAsJsonAsync(url, updateUserDTO2);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
        }

        // Test aktualizacji nie własnego użytkownika
        [Fact]
        public async Task UpdateUser_NotOwn_ShouldReturnForbidden()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var otherUser = await AddUserToDatabaseAsync();
            var url = $"{apiURL}/{otherUser.Id}";

            var updateUserDTO = GetUpdateUserDTOAsync();
            
            // Act
            var response = await client.PutAsJsonAsync(url, updateUserDTO);
            
            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Test aktualizacji nieistniejącego użytkownika
        [Fact]
        public async Task UpdateUser_Nonexistent_ShouldReturnForbidden()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/999";

            var updateUserDTO = GetUpdateUserDTOAsync();
            // Act
            var response = await client.PutAsJsonAsync(url, updateUserDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Test nieautoryzowanej aktualizacji użytkownika
        [Fact]
        public async Task UpdateUser_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = await AddUserToDatabaseAsync();
            var url = $"{apiURL}/{user.Id}";
            var updateUserDTO = GetUpdateUserDTOAsync();
            
            // Act
            var response = await _unauthorizedClient.PutAsJsonAsync(url, updateUserDTO);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego usuwania użytkownika
        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/{userId}";
            
            // Act
            var response = await client.DeleteAsync(url);
            
            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Sprawdź, czy użytkownik został usunięty z bazy danych
            var userInDb = await GetUserIfExistsAsync(userId);
            Assert.Null(userInDb);
        }

        // Test usuwania nieistniejącego użytkownika
        [Fact]
        public async Task DeleteUser_Nonexistent_ShouldReturnForbidden()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/999";

            // Act
            var response = await client.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Test usuwania nie własnego użytkownika
        [Fact]
        public async Task DeleteUser_NotOwn_ShouldReturnForbidden()
        {
            // Arrange
            var (client, userId) = await GetAuthenticatedClientAsync();
            var otherUser = await AddUserToDatabaseAsync();
            var url = $"{apiURL}/{otherUser.Id}";
            
            // Act
            var response = await client.DeleteAsync(url);
            
            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Test nieautoryzowanego usuwania użytkownika
        [Fact]
        public async Task DeleteUser_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            var user = await AddUserToDatabaseAsync();
            var url = $"{apiURL}/{user.Id}";

            // Act
            var response = await _unauthorizedClient.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private CreateUserDTO GetCreateUserDTOAsync(string? username = "Test", string? email = "test@example.com", string? password = "TestPassword123!")
        {
            var dto = new CreateUserDTO
            {
                Username = username,
                Email = email,
                Password = password
            };

            return dto;
        }

        private UpdateUserDTO GetUpdateUserDTOAsync(string? username = "Updated Test", string? email = "updatedTest@example.com", string? password = "TestPassword123!")
        {
            var dto = new UpdateUserDTO
            {
                Username = username,
                Email = email,
                Password = password
            };

            return dto;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task InitializeAsync()
        {
            await ResetDatabaseAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
