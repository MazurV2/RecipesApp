using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Ingredient;
using RecipesApi.Entities;
using RecipesApi.Services.Interfaces;

namespace RecipesApi.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly AppDbContext _context;

        public IngredientService(AppDbContext context)
        {
            _context = context;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<IEnumerable<IngredientDTO>> GetIngredientsAsync()
        {
            // Pobierz wszystkie składniki, przekształć na DTO i zwróć ich listę
            var ingredients = await _context.Ingredients
                .Select(i => new IngredientDTO
                {
                    Id = i.Id,
                    Name = i.Name
                })
                .ToListAsync();

            return ingredients;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        
        public async Task<IngredientDTO> GetIngredientByIdAsync(int id)
        {
            // Znajdź składnik o podanym ID
            var ingredient = await GetIngredientDtoById(id);

            if (ingredient == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono składnika o ID {id}");
            }

            return ingredient;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        
        public async Task<IngredientDTO> CreateIngredientAsync(CreateIngredientDTO createIngredientDTO, int userId)
        {
            var ingredientExists = await _context.Ingredients.AnyAsync(i => i.Name.ToLower() == createIngredientDTO.Name.ToLower());

            if (ingredientExists)
            {
                throw new InvalidOperationException("Składnik o podanej nazwie już istnieje.");
            }

            // Utwórz nowy składnik na podstawie danych z DTO
            var ingredient = new Ingredient
            {
                Name = createIngredientDTO.Name
            };

            // Dodaj nowy składnik do bazy danych
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            // Utwórz obiekt DTO do zwrócenia w odpowiedzi
            var ingredientDTO = await GetIngredientDtoById(ingredient.Id);

            if (ingredientDTO == null)
            {
                throw new Exception("Nie udało się utworzyć składnika");
            }

            return ingredientDTO;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<IngredientDTO> UpdateIngredientAsync(int id, UpdateIngredientDTO updateIngredientDTO, int userId)
        {
            // Znajdź składnik o podanym ID
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono składnika o ID {id}");
            }

            // Sprawdź, czy istnieje już składnik o podanej nazwie
            var ingredientWithSameNameExists = await _context.Ingredients.AnyAsync(i => i.Id != id && i.Name.ToLower() == updateIngredientDTO.Name.ToLower());

            if (ingredientWithSameNameExists)
            {
                throw new InvalidOperationException("Składnik o podanej nazwie już istnieje.");
            }

            // Zaktualizuj i zapisz właściwości składnika
            ingredient.Name = updateIngredientDTO.Name;
            await _context.SaveChangesAsync();

            var ingredientDTO = await GetIngredientDtoById(id);

            if (ingredientDTO == null)
            {
                throw new Exception("Nie udało się zaktualizować składnika");
            }

            return ingredientDTO;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
        
        public async Task DeleteIngredientAsync(int id, int userId)
        {
            // Znajdź składnik o podanym ID
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono składnika o ID {id}");
            }

            // Sprawdź czy składnik nie jest powiązany z przepisem
            var isReferenced = await _context.RecipeIngredients
                .AnyAsync(ri => ri.IngredientId == id);

            if (isReferenced)
            {
                throw new InvalidOperationException("Nie można usunąć składnika, ponieważ jest wykorzystywany");
            }

            // Usuń składnik
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private Task<IngredientDTO?> GetIngredientDtoById(int id)
        {
            return _context.Ingredients
                .Where(i => i.Id == id)
                .Select(i => new IngredientDTO
                {
                    Id = i.Id,
                    Name = i.Name
                })
                .FirstOrDefaultAsync();
        }
    }
}
