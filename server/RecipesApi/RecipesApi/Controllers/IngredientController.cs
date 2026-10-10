using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs.Ingredient;
using RecipesApi.Services.Interfaces;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientController : BaseController
    {
        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        // GET: api/Ingredient
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientDTO>>> GetIngredients()
        {
            var ingredients = await _ingredientService.GetIngredientsAsync();

            return Ok(ingredients);
        }

        // GET: api/Ingredient/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientDTO>> GetIngredient(int id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);

            return Ok(ingredient);
        }

        // POST: api/Ingredient
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<IngredientDTO>> CreateIngredient(CreateIngredientDTO createIngredientDTO)
        {
            var userId = GetUserIdFromClaims();

            var ingredientDTO = await _ingredientService.CreateIngredientAsync(createIngredientDTO, userId);

            // Zwróć odpowiedź z kodem 201 Created i lokalizacją nowo utworzonego zasobu
            return CreatedAtAction(nameof(GetIngredient), new { id = ingredientDTO.Id }, ingredientDTO);
        }

        // PUT: api/Ingredient/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<IngredientDTO>> UpdateIngredient(int id, UpdateIngredientDTO updateIngredientDTO)
        {
            var userId = GetUserIdFromClaims();

            var ingredientDTO = await _ingredientService.UpdateIngredientAsync(id, updateIngredientDTO, userId);

            return Ok(ingredientDTO);
        }

        // DELETE: api/Ingredient/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var userId = GetUserIdFromClaims();

            await _ingredientService.DeleteIngredientAsync(id, userId);

            return NoContent();
        }
    }
}
