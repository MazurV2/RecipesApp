using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Ingredient;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;
using RecipesApi.Entities;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecipeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDTO>>> GetRecipes()
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Steps)
                .Select(r => new RecipeDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
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

            return Ok(recipes);
        }

        // GET: api/Recipe/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            var recipe = await GetRecipeDtoById(id);

            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        // POST: api/Recipe
        [HttpPost]
        public async Task<ActionResult<RecipeDTO>> CreateRecipe(CreateRecipeDTO createRecipeDTO)
        {
            // Utwórz nowy przepis na podstawie danych z DTO
            var recipe = new Recipe
            {
                Title = createRecipeDTO.Title,
                Description = createRecipeDTO.Description,
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
                        StepNumber = s.StepNumber,
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
        public async Task<ActionResult<RecipeDTO>> UpdateRecipe(int id, UpdateRecipeDTO updateRecipeDTO)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            recipe.Title = updateRecipeDTO.Title;
            recipe.Description = updateRecipeDTO.Description;
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
                    StepNumber = s.StepNumber,
                    Description = s.Description
                }).ToList();

            // Zapisz zmiany w bazie danych
            await _context.SaveChangesAsync();

            var recipeDTO = await GetRecipeDtoById(id);

            return Ok(recipe);
        }

        // DELETE: api/Recipe/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Usuń powiązane składniki przepisu
            _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);

            // Usuń powiązane kroki przepisu
            _context.Steps.RemoveRange(recipe.Steps);

            // Usuń przepis
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        private Task<RecipeDTO?> GetRecipeDtoById(int id)
        {
            return _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Steps)
                .Where(r => r.Id == id)
                .Select(r => new RecipeDTO 
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
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
    }
}
