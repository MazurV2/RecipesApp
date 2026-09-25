using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipesApi.DTOs.Auth;
using RecipesApi.Entities;
using System.Collections;
using System.Net.Http.Json;
using System.Web;

namespace RecipesApi.Tests.Integration_Tests
{
    public class BaseIntegrationTests
    {
        protected readonly HttpClient _unauthorizedClient;
        protected readonly CustomWebApplicationFactory<Program> _factory;

        public BaseIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        // Stwórz użytkownika z tokenem JWT
        protected async Task<(HttpClient Client, int UserId)> GetAuthenticatedClientAsync()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var uniqueId = Guid.NewGuid().ToString("N")[..5];
            var password = "TestPassword123!";

            // Dodaj użytkownika do bazy danych
            var user = await AddUserToDatabaseAsync();

            // Zaloguj się, żeby uzyskać token JWT
            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = user.Username,
                Password = password
            };

            var response = await client.PostAsJsonAsync("/api/Auth/login", loginDTO);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
            var token = authResponse.Token;

            // Dodaj token JWT do nagłówka
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return (client, user.Id);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Dodaj użytkownika do bazy danych
        protected async Task<User> AddUserToDatabaseAsync(string? username = null, string? email = null, string password = "TestPassword123!")
        {
            // Sprawdź, czy użytkownik o podanej nazwie już istnieje
            if (username != null && await GetUserIfExistsAsync(username) is User existingUser)
            {
                return existingUser;
            }

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Wygeneruj unikatowe ID dla użytkownika
            var uniqueId = Guid.NewGuid().ToString("N")[..5];

            var user = new User
            {
                Username = username ?? $"user_{uniqueId}",
                Email = email ?? $"user_{uniqueId}@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }

        // Sprawdź, czy użytkownik istnieje w bazie danych
        protected async Task<User?> GetUserIfExistsAsync(string username)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await dbContext.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
            return user;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Dodaj składnik do bazy danych
        protected async Task<Ingredient> AddIngredientToDatabaseAsync(string name = "Test Ingredient")
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Wygeneruj unikatowe ID dla użytkownika
            var uniqueId = Guid.NewGuid().ToString("N")[..5];

            var ingredient = new Ingredient
            {
                Name = $"{name}_{uniqueId}"
            };

            dbContext.Ingredients.Add(ingredient);
            await dbContext.SaveChangesAsync();

            return ingredient;
        }

        // Sprawdź, czy składnik istnieje w bazie danych
        protected async Task<Ingredient?> GetIngredientIfExists(string name)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var ingredient = await dbContext.Ingredients.Where(i => i.Name == name).FirstOrDefaultAsync();
            return ingredient;
        } 

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Dodaj przepis do bazy danych
        protected async Task<Recipe> AddRecipeToDatabaseAsync(
            int? userId = null,
            string title = "Test title",
            string description = "Test description",
            ICollection<RecipeIngredient>? recipeIngredients = null)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var recipe = new Recipe
            {
                UserId = userId,
                Title = title,
                Description = description
            };

            if (recipeIngredients != null)
            {
                recipe.RecipeIngredients = recipeIngredients.Select(ri => new RecipeIngredient
                {
                    IngredientId = ri.IngredientId,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList();
            }

            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();

            return recipe;
        }

        // Sprawdź, czy przepis istnieje w bazie danych
        protected async Task<Recipe?> GetRecipeIfExistsAsync(string title)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var recipe = await dbContext.Recipes.Where(r => r.Title == title).FirstOrDefaultAsync();
            return recipe;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        protected string ToQueryString(object obj)
        {
            // Wyciągnij wszystkie parametry
            var properties = obj.GetType().GetProperties()
                .Where(p => p.GetValue(obj) != null)
                .Select(p =>
                {
                    var value = p.GetValue(obj);

                    // Rozbij złożone pola
                    if (value is IEnumerable enumerable && !(value is string))
                    {
                        var items = enumerable.Cast<object>()
                            .Select(i => $"{p.Name}={HttpUtility.UrlEncode(i.ToString())}");

                        return string.Join("&", items);
                    }

                    return $"{p.Name}={HttpUtility.UrlEncode(value.ToString())}";
                });

            return string.Join("&", properties);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        // Wyczyść bazę danych
        protected async Task ResetDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Recipes.RemoveRange(dbContext.Recipes);
            dbContext.Users.RemoveRange(dbContext.Users);
            dbContext.Ingredients.RemoveRange(dbContext.Ingredients);

            await dbContext.SaveChangesAsync();
        }
    }
}
