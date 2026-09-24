using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;
using RecipesApi.Entities;
using System.Net;
using System.Text;

namespace RecipesApi.Tests.Integration_Tests
{
    public class RecipeControllerTests : BaseIntegrationTests, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        public RecipeControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private async Task<CreateRecipeDTO> PrepareCreateRecipeDTOAsync(
            string? title = "Test Recipe",
            string? description = "Test Description.",
            bool includeRecipeIngredients = true,
            bool includeSteps = true,
            int calories = 100,
            bool includeDifficulty = true,
            DifficultyLevel difficultyLevel = DifficultyLevel.Easy)
        {
            var ingredient = await AddIngredientToDatabaseAsync();

            var recipeIngredientList = includeRecipeIngredients ?
                new List<CreateRecipeIngredientDTO>
                {
                    new CreateRecipeIngredientDTO
                    {
                        IngredientId = ingredient.Id,
                        Amount = 1,
                        Unit = "g"
                    }
                }
                : null;

            var stepList = includeSteps ?
                new List<CreateStepDTO>
                {
                    new CreateStepDTO { Description = "Test Step."}
                }
                : null;

            var createRecipeDTO = new CreateRecipeDTO
            {
                Title = title,
                Description = description,
                RecipeIngredients = recipeIngredientList,
                Steps = stepList,
                Calories = calories
            };

            if (includeDifficulty) createRecipeDTO.Difficulty = difficultyLevel;

            return createRecipeDTO;
        }

        private async Task<UpdateRecipeDTO> PrepareUpdateRecipeDTOAsync(
            string? title = "Updated Test Recipe",
            string? description = "Updated Test Description.",
            bool includeRecipeIngredients = true,
            bool includeSteps = true,
            int calories = 100,
            DifficultyLevel difficultyLevel = DifficultyLevel.Easy)
        {
            var ingredient = await AddIngredientToDatabaseAsync();

            var recipeIngredientList = includeRecipeIngredients ?
                new List<UpdateRecipeIngredientDTO>
                {
                    new UpdateRecipeIngredientDTO
                    {
                        IngredientId = ingredient.Id,
                        Amount = 1,
                        Unit = "g"
                    }
                }
                : null;

            var stepList = includeSteps ?
                new List<UpdateStepDTO>
                {
                    new UpdateStepDTO { Description = "Updated Test Step."}
                }
                : null;

            var updateRecipeDTO = new UpdateRecipeDTO
            {
                Title = title,
                Description = description,
                RecipeIngredients = recipeIngredientList,
                Steps = stepList,
                Calories = calories,
                Difficulty = difficultyLevel
            };

            return updateRecipeDTO;
        }

        private MultipartFormDataContent GetRecipeMultipartContent(CreateRecipeDTO dto, byte[]? imageBytes = null, string fileName = "test.jpg")
        {
            var httpContent = new MultipartFormDataContent();

            // Dane przepisu
            httpContent.Add(new StringContent(dto.Title), nameof(dto.Title));
            httpContent.Add(new StringContent(dto.Description), nameof(dto.Description));
            httpContent.Add(new StringContent(dto.Calories.ToString()), nameof(dto.Calories));
            httpContent.Add(new StringContent(((int)dto.Difficulty).ToString()), nameof(dto.Difficulty));

            // Składniki
            var recipeIngredientsList = dto.RecipeIngredients?.ToList() ?? new List<CreateRecipeIngredientDTO>();
            for (int i = 0; i < recipeIngredientsList.Count; i++)
            {
                httpContent.Add(new StringContent(recipeIngredientsList[i].IngredientId.ToString()), $"RecipeIngredients[{i}].IngredientId");
                httpContent.Add(new StringContent(recipeIngredientsList[i].Amount.ToString()), $"RecipeIngredients[{i}].Amount");
                httpContent.Add(new StringContent(recipeIngredientsList[i].Unit), $"RecipeIngredients[{i}].Unit");
            }

            // Kroki instrukcji
            var stepsList = dto.Steps?.ToList() ?? new List<CreateStepDTO>();
            for (int i = 0; i < stepsList.Count; i++)
            {
                httpContent.Add(new StringContent(stepsList[i].Description), $"Steps[{i}].Description");
            }

            // Obraz przepisu
            if (imageBytes != null)
            {
                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                httpContent.Add(fileContent, nameof(dto.Image), fileName);
            }

            return httpContent;
        }

        private MultipartFormDataContent GetRecipeMultipartContent(UpdateRecipeDTO dto, byte[]? imageBytes = null, string fileName = "test.jpg")
        {
            var httpContent = new MultipartFormDataContent();

            httpContent.Add(new StringContent(dto.Title), nameof(dto.Title));
            httpContent.Add(new StringContent(dto.Description), nameof(dto.Description));
            httpContent.Add(new StringContent(dto.Calories.ToString()), nameof(dto.Calories));
            httpContent.Add(new StringContent(((int)dto.Difficulty).ToString()), nameof(dto.Difficulty));

            var recipeIngredientsList = dto.RecipeIngredients?.ToList() ?? new List<UpdateRecipeIngredientDTO>();
            for (int i = 0; i < recipeIngredientsList.Count; i++)
            {
                httpContent.Add(new StringContent(recipeIngredientsList[i].IngredientId.ToString()), $"RecipeIngredients[{i}].IngredientId");
                httpContent.Add(new StringContent(recipeIngredientsList[i].Amount.ToString()), $"RecipeIngredients[{i}].Amount");
                httpContent.Add(new StringContent(recipeIngredientsList[i].Unit), $"RecipeIngredients[{i}].Unit");
            }

            var stepsList = dto.Steps?.ToList() ?? new List<UpdateStepDTO>();
            for (int i = 0; i < stepsList.Count; i++)
            {
                httpContent.Add(new StringContent(stepsList[i].Description), $"Steps[{i}].Description");
            }

            if (imageBytes != null)
            {
                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                httpContent.Add(fileContent, nameof(dto.Image), fileName);
            }

            return httpContent;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego pobierania przepisów
        [Fact]
        public async Task GetRecipes_ShouldReturnOk()
        {
            // Arrange
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test pobrania istniejącego przepisu po id
        [Fact]
        public async Task GetRecipe_ExistingRecipe_ShouldReturnOK()
        {
            // Arrange
            await ResetDatabaseAsync();
            var recipe = await AddRecipeToDatabaseAsync();
            var apiURL = $"/api/Recipe/{recipe.Id}";
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Test pobrania nieistniejącego przepisu po id
        [Fact]
        public async Task GetRecipe_NonexistentRecipe_ShouldReturnNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe/999";
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Act
            var response = await client.GetAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego tworzenia przepisu
        [Fact]
        public async Task CreateRecipe_ValidData_ShouldReturnCreated()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj dane przepisu
            var createRecipeDTO = await PrepareCreateRecipeDTOAsync();

            // Przygotuj obraz przepisu (nagłówek JPEG)
            string testImageName = "testImage.jpg";
            byte[] testImageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };

            var httpContent = GetRecipeMultipartContent(createRecipeDTO, testImageBytes, testImageName);

            // Act
            var response = await client.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Sprawdź, czy przepis został dodany do bazy danych
            var recipeInDb = await GetRecipeIfExistsAsync(createRecipeDTO.Title);

            Assert.NotNull(recipeInDb);
            Assert.Equal(createRecipeDTO.Description, recipeInDb.Description);
        }

        // Test tworzenia przepisu z błędnymi danymi
        [Theory]
        [InlineData("", "Test Description", 100)]
        [InlineData("Test Recipe", "", 100)]
        [InlineData("Test Recipe", "Test Description", -100)]
        public async Task CreateRecipe_InvalidData_ShouldReturnBadRequest(string title, string description, int calories)
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj dane przepisu
            var createRecipeDTO = await PrepareCreateRecipeDTOAsync(
                title: title,
                description: description,
                calories: calories);

            var httpContent = GetRecipeMultipartContent(createRecipeDTO);

            // Act
            var response = await client.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test tworzenia przepisu z nieprawidłowym poziomem trudności
        [Fact]
        public async Task CreateRecipe_InvalidDifficultyLevel_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj dane przepisu
            var createRecipeDTO = await PrepareCreateRecipeDTOAsync(includeDifficulty: false);

            var httpContent = GetRecipeMultipartContent(createRecipeDTO);
            // Wstaw nieprawidłowy poziom trudności
            httpContent.Add(new StringContent("invalidDifficulty"), nameof(createRecipeDTO.Difficulty));

            // Act
            var response = await client.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test tworzenia przepisu bez składników
        [Fact]
        public async Task CreateRecipe_NoRecipeIngredients_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj dane przepisu
            var createRecipeDTO = await PrepareCreateRecipeDTOAsync(includeRecipeIngredients: false);

            var httpContent = GetRecipeMultipartContent(createRecipeDTO);

            // Act
            var response = await client.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test tworzenia przepisu bez kroków
        [Fact]
        public async Task CreateRecipe_NoSteps_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj dane przepisu
            var createRecipeDTO = await PrepareCreateRecipeDTOAsync(includeSteps: false);

            var httpContent = GetRecipeMultipartContent(createRecipeDTO);

            // Act
            var response = await client.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test tworzenia przepisu z błędnym formatem obrazu
        [Fact]
        public async Task CreateRecipe_InvalidImageFormat_ShouldReturnBadRequest()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";
            var (client, userId) = await GetAuthenticatedClientAsync();

            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj dane przepisu
            var createRecipeDTO = await PrepareCreateRecipeDTOAsync();

            var httpContent = GetRecipeMultipartContent(createRecipeDTO, Encoding.UTF8.GetBytes("zwykły tekst"), "test.txt");

            // Act
            var response = await client.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test nieautoryzowanego tworzenia przepisu
        [Fact]
        public async Task CreateRecipe_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe";

            var createRecipeDTO = new CreateRecipeDTO
            {
                Title = "Test Recipe"
            };

            var httpContent = GetRecipeMultipartContent(createRecipeDTO);

            // Act
            var response = await _unauthorizedClient.PostAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnej aktualizaji przepisu
        [Fact]
        public async Task UpdateRecipe_ValidData_ShouldReturnOK()
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();
            var recipe = await AddRecipeToDatabaseAsync(userId: userId);
            var apiURL = $"/api/Recipe/{recipe.Id}";

            // Dodaj składnik wymagany do walidacji
            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj zaktualizowane dane przepisu
            var updateRecipeDTO = await PrepareUpdateRecipeDTOAsync();

            var httpContent = GetRecipeMultipartContent(updateRecipeDTO);

            // Act
            var response = await client.PutAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź, czy przepis został zaktualizowany w bazie danych
            var recipeInDb = await GetRecipeIfExistsAsync(updateRecipeDTO.Title);

            Assert.NotNull(recipeInDb);
            Assert.Equal(updateRecipeDTO.Description, recipeInDb.Description);
        }

        // Test niepoprawnej aktualizacji przepisu
        [Theory]
        [InlineData("", "Updated Test Description", 200)]
        [InlineData("Updated Test Recipe", "", 200)]
        [InlineData("Updated Test Recipe", "Updated Test Description", -200)]
        public async Task UpdateRecipe_InvalidData_ShouldReturnBadRequest(string title, string description, int calories)
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();

            var recipe = await AddRecipeToDatabaseAsync(userId);
            var apiURL = $"/api/Recipe/{recipe.Id}";

            // Dodaj składnik wymagany do walidacji
            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj zaktualizowane dane przepisu
            var updateRecipeDTO = await PrepareUpdateRecipeDTOAsync(title: title, description: description, calories: calories);

            var httpContent = GetRecipeMultipartContent(updateRecipeDTO);

            // Act
            var response = await client.PutAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test aktualizacji nieistniejącego przepisu
        [Fact]
        public async Task UpdateRecipe_NonexistentRecipe_ShouldReturnNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe/999";
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Przygotuj zaktualizowane dane przepisu
            var updateRecipeDTO = await PrepareUpdateRecipeDTOAsync();

            var httpContent = GetRecipeMultipartContent(updateRecipeDTO);

            // Act
            var response = await client.PutAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test aktualizacji nieswojego przepisu
        [Fact]
        public async Task UpdateRecipe_NotOwnedRecipe_ShouldReturnForbid()
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Dodaj innego użytkownika
            var otherUser = await AddUserToDatabaseAsync();

            var recipe = await AddRecipeToDatabaseAsync(userId: otherUser.Id);
            var apiURL = $"/api/Recipe/{recipe.Id}";

            // Dodaj składnik wymagany do walidacji
            var ingredient = await AddIngredientToDatabaseAsync();

            // Przygotuj zaktualizowane dane przepisu
            var updateRecipeDTO = await PrepareUpdateRecipeDTOAsync();

            var httpContent = GetRecipeMultipartContent(updateRecipeDTO);

            // Act
            var response = await client.PutAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            // Sprawdź, czy przepis nie został zaktualizowany w bazie danych
            var recipeInDb = await GetRecipeIfExistsAsync(recipe.Title);
            Assert.NotEqual(updateRecipeDTO.Description, recipeInDb.Description);
        }

        // Test nieautoryzowanej aktualizacji przepisu
        [Fact]
        public async Task UpdateRecipe_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetDatabaseAsync();
            var apiURL = "/api/Recipe/1";

            var updateRecipeDTO = await PrepareUpdateRecipeDTOAsync();

            var httpContent = GetRecipeMultipartContent(updateRecipeDTO);

            // Act
            var response = await _unauthorizedClient.PutAsync(apiURL, httpContent);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Test poprawnego usunięcia przepisu
        [Fact]
        public async Task DeleteRecipe_ShouldReturnNoContent()
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Dodaj przepis do bazy danych
            var recipe = await AddRecipeToDatabaseAsync(userId: userId);
            var apiURL = $"/api/Recipe/{recipe.Id}";

            // Act
            var response = await client.DeleteAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Sprawdź, czy przepis usunięto z bazy danych
            var recipeInDb = await GetRecipeIfExistsAsync(recipe.Title);
            Assert.Null(recipeInDb);
        }

        // Test usunięcia nieistniejącego przepisu
        [Fact]
        public async Task DeleteRecipe_NonexistentRecipe_ShouldReturnNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();
            var apiURL = $"/api/Recipe/1";

            // Act
            var response = await client.DeleteAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Test usunięcia nieswojego przepisu
        [Fact]
        public async Task DeleteRecipe_NotOwnedRecipe_ShouldReturnForbid()
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Dodaj innego użytkownika
            var otherUser = await AddUserToDatabaseAsync();

            // Dodaj przepis do bazy danych
            var recipe = await AddRecipeToDatabaseAsync(userId: otherUser.Id);
            var apiURL = $"/api/Recipe/{recipe.Id}";

            // Act
            var response = await client.DeleteAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            // Sprawdź, czy przepis pozostał w bazie danych
            var recipeInDb = await GetRecipeIfExistsAsync(recipe.Title);
            Assert.NotNull(recipeInDb);
        }

        // Test nieautoryzowanego usunięcia przepisu
        [Fact]
        public async Task DeleteRecipe_NoJWT_ShouldReturnUnauthorized()
        {
            // Arrange
            await ResetDatabaseAsync();
            var (client, userId) = await GetAuthenticatedClientAsync();

            // Dodaj przepis do bazy danych
            var recipe = await AddRecipeToDatabaseAsync(userId: userId);
            var apiURL = $"/api/Recipe/{recipe.Id}";

            // Act
            var response = await _unauthorizedClient.DeleteAsync(apiURL);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            // Sprawdź, czy przepis pozostał w bazie danych
            var recipeInDb = await GetRecipeIfExistsAsync(recipe.Title);
            Assert.NotNull(recipeInDb);
        }
    }
}
