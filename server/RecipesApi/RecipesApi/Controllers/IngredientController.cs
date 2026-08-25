using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Ingredient;
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
            // Pobierz wszystkie składniki, przekształć na DTO i zwróć ich listę
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
            // Znajdź składnik o podanym ID
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            // Utwórz obiekt DTO do zwrócenia w odpowiedzi
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
            // Utwórz nowy składnik na podstawie danych z DTO
            var ingredient = new Ingredient
            {
                Name = createIngredientDTO.Name
            };

            // Dodaj nowy składnik do bazy danych
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            // Utwórz obiekt DTO do zwrócenia w odpowiedzi
            var ingredientDTO = new IngredientDTO
            {
                Id = ingredient.Id,
                Name = ingredient.Name
            };

            // Zwróć odpowiedź z kodem 201 Created i lokalizacją nowo utworzonego zasobu
            return CreatedAtAction(nameof(GetIngredient), new { id = ingredient.Id }, ingredientDTO);
        }

        // PUT: api/Ingredient/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<IngredientDTO>> UpdateIngredient(int id, UpdateIngredientDTO updateIngredientDTO)
        {
            // Znajdź składnik o podanym ID
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            // Zaktualizuj i zapisz właściwości składnika
            ingredient.Name = updateIngredientDTO.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Ingredient/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            // Znajdź składnik o podanym ID
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            // Usuń składnik
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
