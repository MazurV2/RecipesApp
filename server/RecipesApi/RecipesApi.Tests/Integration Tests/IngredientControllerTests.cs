
using RecipesApi.DTOs.Ingredient;
using RecipesApi.DTOs.Recipe;
using RecipesApi.Entities;
using System.Net;
using System.Net.Http.Json;
using System.Xml.Linq;

namespace RecipesApi.Tests.Integration_Tests
{
    public class IngredientControllerTests : BaseIntegrationTests, IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private string apiURL = "/api/Ingredient";

        public IngredientControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test pobierania składników
        [Fact]
        public async Task GetIngredients_ShouldReturnOK()
        {
            // Arrange

            // Dodaj przykładowe składniki
            await AddIngredientToDatabaseAsync("Chleb");
            await AddIngredientToDatabaseAsync("Ogórek");
            await AddIngredientToDatabaseAsync("Pomidor");

            // Act
            var response = await _unauthorizedClient.GetAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var results = await response.Content.ReadFromJsonAsync<IEnumerable<IngredientDTO>>();
            Assert.NotNull(results);
            Assert.Equal(3, results.Count());
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test pobierania składnika po id
        [Fact]
        public async Task GetIngredient_ShouldReturnOK()
        {
            // Arrange
            var ingredient = await AddIngredientToDatabaseAsync();
            var url = $"{apiURL}/{ingredient.Id}";

            // Act
            var response = await _unauthorizedClient.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Test pobrania nieistniejącego składnika
        [Fact]
        public async Task GetIngredient_NonexistentIngredient_ShouldReturnNotFound()
        {
            // Arrange
            var url = $"{apiURL}/999";

            // Act
            var response = await _unauthorizedClient.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego dodawania składnika
        [Fact]
        public async Task CreateIngredient_ValidData_ShouldReturnOK()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            var createIngredientDTO = GetCreateIngredientDTOAsync();

            // Act
            var response = await client.PostAsJsonAsync(apiURL, createIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Sprawdź, czy skłądnik został dodany do bazy danych
            var ingredientInDb = await GetIngredientIfExists(createIngredientDTO.Name);

            Assert.NotNull(ingredientInDb);
        }

        // Test dodawania składnika z błędami
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateIngredient_InvalidData_ShouldReturnBadRequest(string? name)
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            var createIngredientDTO = GetCreateIngredientDTOAsync(name);

            // Act
            var response = await client.PostAsJsonAsync(apiURL, createIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var ingredientInDb = await GetIngredientIfExists(createIngredientDTO.Name);
            Assert.Null(ingredientInDb);
        }

        // Test dodawania istniejącego składnika
        [Fact]
        public async Task CreateIngredient_AlreadyExists_ShouldReturnBadRequest()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            var ingredientInDb = await AddIngredientToDatabaseAsync();
            var ingredient = GetCreateIngredientDTOAsync(ingredientInDb.Name);

            // Act
            var response = await client.PostAsJsonAsync(apiURL, ingredient);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test nieautoryzowanego dodawania składnika
        [Fact]
        public async Task CreatIngredient_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            var ingredient = GetCreateIngredientDTOAsync();

            // Act
            var response = await _unauthorizedClient.PostAsJsonAsync(apiURL, ingredient);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var ingredientInDb = await GetIngredientIfExists(ingredient.Name);
            Assert.Null(ingredientInDb);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnej aktualizacji składnika
        [Fact]
        public async Task UpdateIngredient_ValidData_ShouldReturnOK()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            // Dodaj składnik do bazy danych
            var ingredient = await AddIngredientToDatabaseAsync();
            var url = $"{apiURL}/{ingredient.Id}";

            var updateIngredientDTO = GetUpdateIngredientDTOAsync();

            // Act
            var response = await client.PutAsJsonAsync(url, updateIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź czy w bazie zmieniono nazwę składnika
            var ingredientInDb = await GetIngredientIfExists(updateIngredientDTO.Name);
            Assert.NotNull(ingredientInDb);
        }

        // Test aktualizacji składnika z błędami
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task UpdateIngredient_InvalidData_ShouldReturnBadRequest(string? name)
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            // Dodaj składnik do bazy danych
            var ingredient = await AddIngredientToDatabaseAsync();
            var url = $"{apiURL}/{ingredient.Id}";

            var updateIngredientDTO = GetUpdateIngredientDTOAsync(name);

            // Act
            var response = await client.PutAsJsonAsync(url, updateIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Sprawdź czy w bazie nie zmieniono nazwy składnika
            var ingredientInDb = await GetIngredientIfExists(updateIngredientDTO.Name);
            Assert.Null(ingredientInDb);
        }

        // Test aktualizacji nieistniejącego składnika
        [Fact]
        public async Task UpdateIngredient_NonexistentIngredient_ShouldReturnNotFound()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();
            var url = $"{apiURL}/999";

            var updateIngredientDTO = GetUpdateIngredientDTOAsync();

            // Act
            var response = await client.PutAsJsonAsync(url, updateIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Sprawdź czy w bazie nie zmieniono nazwy składnika
            var ingredientInDb = await GetIngredientIfExists(updateIngredientDTO.Name);
            Assert.Null(ingredientInDb);
        }

        // Test aktualizacji składnika przypisanego do przepisu
        [Fact]
        public async Task UpdateIngredient_IngredientReferencedByRecipe_ShouldReturnOK()
        {
            // Arrange
            var (client, _) = await GetAuthenticatedClientAsync();

            // Dodaj składnik powiązany z przepisem
            var ingredient = await AddRecipeWithIngredientReference();
            var url = $"{apiURL}/{ingredient.Id}";

            var updateIngredientDTO = GetUpdateIngredientDTOAsync();

            // Act
            var response = await client.PutAsJsonAsync(url, updateIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź czy w bazie zmieniono nazwę składnika
            var ingredientInDb = await GetIngredientIfExists(updateIngredientDTO.Name);
            Assert.NotNull(ingredientInDb);
        }

        // Test nieautoryzowanej aktualizacji składnika
        [Fact]
        public async Task UpdateIngredient_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange

            // Dodaj składnik do bazy danych
            var ingredient = await AddIngredientToDatabaseAsync();
            var url = $"{apiURL}/{ingredient.Id}";

            var updateIngredientDTO = GetUpdateIngredientDTOAsync();

            // Act
            var response = await _unauthorizedClient.PutAsJsonAsync(url, updateIngredientDTO);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            // Sprawdź czy w bazie nie zmieniono nazwy składnika
            var ingredientInDb = await GetIngredientIfExists(updateIngredientDTO.Name);
            Assert.Null(ingredientInDb);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego usuwania składnika
        [Fact]
        public async Task DeleteIngredient_ShouldReturnNoContent()
        {
            // Arrange
            var ingredient = await AddIngredientToDatabaseAsync();
            var url = $"{apiURL}/{ingredient.Id}";
            var (client, _) = await GetAuthenticatedClientAsync();

            // Act
            var response = await client.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var ingredientInDb = await GetIngredientIfExists(ingredient.Name);
            Assert.Null(ingredientInDb);
        }

        // Test usuwania nieistniejącego składnika
        [Fact]
        public async Task DeleteIngredient_NonexistentIngredient_ShouldReturnNotFound()
        {
            // Arrange
            var url = $"{apiURL}/999";
            var (client, _) = await GetAuthenticatedClientAsync();

            // Act
            var response = await client.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test usuwania składnika przypisanego do przepisu
        [Fact]
        public async Task DeleteIngredient_IngredientReferencedByRecipe_ShouldReturnConflict()
        {
            // Arrange
            var ingredient = await AddRecipeWithIngredientReference();
            var url = $"{apiURL}/{ingredient.Id}";
            var (client, _) = await GetAuthenticatedClientAsync();

            // Act
            var response = await client.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var ingredientInDb = await GetIngredientIfExists(ingredient.Name);
            Assert.NotNull(ingredientInDb);
        }

        // Test nieautoryzowanego usuwania składnika
        [Fact]
        public async Task DeleteIngredient_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            var ingredient = await AddIngredientToDatabaseAsync();
            var url = $"{apiURL}/{ingredient.Id}";

            // Act
            var response = await _unauthorizedClient.DeleteAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var ingredientInDb = await GetIngredientIfExists(ingredient.Name);
            Assert.NotNull(ingredientInDb);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private CreateIngredientDTO GetCreateIngredientDTOAsync(string? name = "Test Ingredient")
        {
            var dto = new CreateIngredientDTO
            {
                Name = name
            };

            return dto;
        }

        private UpdateIngredientDTO GetUpdateIngredientDTOAsync(string? name = "Updated Test Ingredient")
        {
            var dto = new UpdateIngredientDTO
            {
                Name = name
            };

            return dto;
        }

        private async Task<Ingredient> AddRecipeWithIngredientReference()
        {
            // Dodaj składnik do bazy danych
            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj powiązanie składnika z przepisem
            var recipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                    {
                        IngredientId = ingredient.Id,
                        Amount = 3,
                        Unit = "g"
                    }
            };

            // Dodaj przepis do bazy danych
            var recipe = await AddRecipeToDatabaseAsync(recipeIngredients: recipeIngredients);

            return ingredient;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task InitializeAsync()
        {
            await ResetDatabaseAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
