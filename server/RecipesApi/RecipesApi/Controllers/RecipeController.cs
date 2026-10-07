using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs.Recipe;
using RecipesApi.Pagination;
using RecipesApi.Services.Interfaces;
using System.Security.Claims;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<PagedResults<RecipeDTO>>> GetRecipes([FromQuery] RecipeQueryDTO queryDTO)
        {
            var results = await _recipeService.GetRecipesAsync(queryDTO);

            return Ok(results);
        }

        // GET: api/Recipe/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);

            return Ok(recipe);
        }

        // POST: api/Recipe
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RecipeDTO>> CreateRecipe([FromForm] CreateRecipeDTO createRecipeDTO)
        {
            var userId = GetUserIdFromClaims();

            var recipeDTO = await _recipeService.CreateRecipeAsync(createRecipeDTO, userId);

            return CreatedAtAction(nameof(GetRecipe), new { id = recipeDTO.Id }, recipeDTO);
        }

        // PUT: api/Recipe/{id}
        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RecipeDTO>> UpdateRecipe(int id, [FromForm] UpdateRecipeDTO updateRecipeDTO)
        {
            var userId = GetUserIdFromClaims();

            var recipeDTO = await _recipeService.UpdateRecipeAsync(id, updateRecipeDTO, userId);

            return Ok(recipeDTO);
        }

        // DELETE: api/Recipe/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = GetUserIdFromClaims();

            await _recipeService.DeleteRecipeAsync(id, userId);

            return NoContent();
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");
            }

            return int.Parse(userIdClaim.Value);
        }
    }
}
