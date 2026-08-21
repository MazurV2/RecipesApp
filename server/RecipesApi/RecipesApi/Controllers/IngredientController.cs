using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs;
using RecipesApi.Entities;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngredientController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ingredient
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientDTO>>> GetIngredients()
        {
            var ingredients = await _context.Ingredients
                .Select(i => new IngredientDTO
                {
                    Id = i.Id,
                    Name = i.Name
                })
                .ToListAsync();

            return Ok(ingredients);
        }

        // GET: api/Ingredient/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientDTO>> GetIngredient(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            var ingredientDTO = new IngredientDTO
            {
                Id = ingredient.Id,
                Name = ingredient.Name
            };

            return Ok(ingredientDTO);
        }

        // POST: api/Ingredient
        [HttpPost]
        public async Task<ActionResult<IngredientDTO>> CreateIngredient(CreateIngredientDTO createIngredientDTO)
        {
            // Utwórz nowy obiekt Ingredient
            var ingredient = new Ingredient
            {
                Name = createIngredientDTO.Name
            };

            // Dodaj nowy obiekt Ingredient do kontekstu i zapisz zmiany w bazie danych
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            // Utwórz obiekt IngredientDTO do zwrócenia w odpowiedzi
            var ingredientDTO = new IngredientDTO
            {
                Id = ingredient.Id,
                Name = ingredient.Name
            };

            // Zwróć odpowiedź z kodem 201 Created i lokalizacją nowo utworzonego zasobu
            return CreatedAtAction(nameof(GetIngredient), new { id = ingredient.Id }, ingredientDTO);
        }
    }
}
