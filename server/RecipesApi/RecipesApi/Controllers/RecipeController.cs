using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs.Recipe;
using RecipesApi.Pagination;
using RecipesApi.Services.Interfaces;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : BaseController
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
            var recipeDTOs = await _recipeService.GetRecipesAsync(queryDTO);

            return Ok(recipeDTOs);
        }

        // GET: api/Recipe/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            var recipeDTO = await _recipeService.GetRecipeByIdAsync(id);

            return Ok(recipeDTO);
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
    }
}
