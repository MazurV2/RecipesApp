using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;
using RecipesApi.Entities;
using RecipesApi.Pagination;
using RecipesApi.Services.Interfaces;
using System.Security.Claims;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private const string _recipeImagesFolder = "images/recipes";

        public RecipeController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<PagedResults<RecipeDTO>>> GetRecipes([FromQuery] RecipeQueryDTO queryDTO)
        {
            // Stwórz bazowe zapytanie do bazy danych
            var query = _context.Recipes.AsQueryable();

            // Filtruj po szukanej frazie
            if (!string.IsNullOrWhiteSpace(queryDTO.SearchTerm))
            {
                var searchTerm = queryDTO.SearchTerm.ToLower();
                query = query.Where(r => 
                    r.Title.ToLower().Contains(searchTerm) || 
                    r.Description.ToLower().Contains(searchTerm)
                );
            }

            // Filtruj po kaloriach
            if (queryDTO.MinCalories.HasValue)
            {
                query = query.Where(r => r.Calories >= queryDTO.MinCalories.Value);
            }
            
            if (queryDTO.MaxCalories.HasValue)
            {
                query = query.Where(r => r.Calories <= queryDTO.MaxCalories.Value);
            }

            // Filtruj po poziomie trudności
            if (queryDTO.MinDifficulty.HasValue)
            {
                query = query.Where(r => (int)r.Difficulty >= queryDTO.MinDifficulty.Value);
            }

            if (queryDTO.MaxDifficulty.HasValue)
            {
                query = query.Where(r => (int)r.Difficulty <= queryDTO.MaxDifficulty.Value);
            }

            // Sortuj po wybranym polu i kierunku
            query = queryDTO.SortBy?.ToLower() switch
            {
                "title" => queryDTO.SortDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "calories" => queryDTO.SortDescending ? query.OrderByDescending(r => r.Calories) : query.OrderBy(r => r.Calories),
                "difficulty" => queryDTO.SortDescending ? query.OrderByDescending(r => r.Difficulty) : query.OrderBy(r => r.Difficulty),
                _ => queryDTO.SortDescending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
            };

            // Pobierz całkowitą liczbę przepisów po zastosowaniu filtrów
            var totalCount = await query.CountAsync();

            // Zastosuj paginację i przekształć wyniki na DTO
            var recipes = await query
                .Skip(queryDTO.PageSize * (queryDTO.PageNumber - 1))
                .Take(queryDTO.PageSize)
                .Select(r => new RecipeDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    RecipeIngredients = r.RecipeIngredients
                        .Select(ri => new RecipeIngredientDTO
                        {
                            IngredientId = ri.IngredientId,
                            IngredientName = ri.Ingredient.Name,
                            Amount = ri.Amount,
                            Unit = ri.Unit
                        }).ToList(),
                    Steps = r.Steps
                        .Select(s => new StepDTO
                        {
                            Id = s.Id,
                            StepNumber = s.StepNumber,
                            Description = s.Description,
                        }).ToList(),
                    Calories = r.Calories,
                    Difficulty = r.Difficulty.ToString()
                })
                .ToListAsync();

            var results = new PagedResults<RecipeDTO>(recipes, totalCount, queryDTO.PageNumber, queryDTO.PageSize);

            return Ok(results);
        }

        // GET: api/Recipe/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            var recipe = await GetRecipeDtoById(id);

            if (recipe == null) return NotFound();

            return Ok(recipe);
        }

        // POST: api/Recipe
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RecipeDTO>> CreateRecipe([FromForm] CreateRecipeDTO createRecipeDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            // Sprawdź czy wprowadzone składniki istnieją
            var ingredientIds = createRecipeDTO.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
            var result = await CheckForMissingIngredients(ingredientIds);
            if (result != null) return result;

            // Zapisz obraz przepisu, jeśli został przesłany
            string? imageUrl = await SaveImageGetUrl(createRecipeDTO.Image);

            // Utwórz nowy przepis na podstawie danych z DTO
            var recipe = new Recipe
            {
                UserId = int.Parse(userId),
                Title = createRecipeDTO.Title,
                Description = createRecipeDTO.Description,
                ImageUrl = imageUrl,
                RecipeIngredients = createRecipeDTO.RecipeIngredients
                    .Select(ri => new RecipeIngredient
                    {
                        IngredientId = ri.IngredientId,
                        Amount = ri.Amount,
                        Unit = ri.Unit
                    }).ToList(),
                Steps = createRecipeDTO.Steps
                    .Select(s => new Step
                    {
                        Description = s.Description
                    }).ToList(),
                Calories = createRecipeDTO.Calories,
                Difficulty = createRecipeDTO.Difficulty,
            };

            // Dodaj nowy przepis do bazy danych
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            var recipeDTO = await GetRecipeDtoById(recipe.Id);

            return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipeDTO);
        }

        // PUT: api/Recipe/{id}
        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RecipeDTO>> UpdateRecipe(int id, [FromForm] UpdateRecipeDTO updateRecipeDTO)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            // Sprawdź czy przepis należy do zalogowanego użytkownika
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();
            if (recipe.UserId != int.Parse(userId)) return Forbid();

            // Zapisz nowy i usuń stary obraz, jeśli został przesłany
            string? imageUrl = await SaveImageGetUrl(updateRecipeDTO.Image);
            if (imageUrl != null)
            {
                _fileService.DeleteFile(recipe.ImageUrl);
            }

            // Sprawdź czy wprowadzone składniki istnieją
            var ingredientIds = updateRecipeDTO.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
            var result = await CheckForMissingIngredients(ingredientIds);
            if (result != null) return result;

            recipe.Title = updateRecipeDTO.Title;
            recipe.Description = updateRecipeDTO.Description;
            recipe.ImageUrl = imageUrl ?? recipe.ImageUrl;
            recipe.Calories = updateRecipeDTO.Calories;
            recipe.Difficulty = updateRecipeDTO.Difficulty;

            // Zaktualizuj składniki przepisu
            _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
            recipe.RecipeIngredients = updateRecipeDTO.RecipeIngredients
                .Select(ri => new RecipeIngredient
                {
                    IngredientId = ri.IngredientId,
                    Amount = ri.Amount,
                    Unit = ri.Unit
                }).ToList();

            // Zaktualizuj kroki przepisu
            _context.Steps.RemoveRange(recipe.Steps);
            recipe.Steps = updateRecipeDTO.Steps
                .Select(s => new Step
                {
                    Description = s.Description
                }).ToList();

            // Zapisz zmiany w bazie danych
            await _context.SaveChangesAsync();

            var recipeDTO = await GetRecipeDtoById(id);

            return Ok(recipeDTO);
        }

        // DELETE: api/Recipe/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            // Sprawdź czy przepis należy do zalogowanego użytkownika
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();
            if (recipe.UserId != int.Parse(userId)) return Forbid();

            // Usuń obraz przepisu, jeśli istnieje
            _fileService.DeleteFile(recipe.ImageUrl);

            // Usuń przepis
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<RecipeDTO?> GetRecipeDtoById(int id)
        {
            return _context.Recipes
                .Where(r => r.Id == id)
                .Select(r => new RecipeDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    RecipeIngredients = r.RecipeIngredients
                        .Select(ri => new RecipeIngredientDTO
                        {
                            IngredientId = ri.IngredientId,
                            IngredientName = ri.Ingredient.Name,
                            Amount = ri.Amount,
                            Unit = ri.Unit
                        }).ToList(),
                    Steps = r.Steps
                        .Select(s => new StepDTO
                        {
                            Id = s.Id,
                            StepNumber = s.StepNumber,
                            Description = s.Description,
                        }).ToList(),
                    Calories = r.Calories,
                    Difficulty = r.Difficulty.ToString()
                })
                .FirstOrDefaultAsync();
        }

        private async Task<string?> SaveImageGetUrl(IFormFile? image)
        {
            if (image != null && image.Length > 0)
            {
                var imageUrl = await _fileService.SaveFileAsync(image, _recipeImagesFolder);
                return imageUrl;
            }
            return null;
        }

        private async Task<ActionResult?> CheckForMissingIngredients(List<int> ingredientIds)
        {
            // Sprawdź czy podane id składników istnieją w bazie
            var existingIngredientIds = await _context.Ingredients
                .Where(i => ingredientIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();

            // Zbierz nieistniejące składniki
            var missingIngredientIds = ingredientIds.Except(existingIngredientIds).ToList();

            if (missingIngredientIds.Any())
            {
                return BadRequest(new
                {
                    error = "Wprowadzono nieprawidłowe Id składników",
                    missing = missingIngredientIds
                });
            }

            return null;
        }
    }
}
