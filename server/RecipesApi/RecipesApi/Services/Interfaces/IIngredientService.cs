using RecipesApi.DTOs.Ingredient;

namespace RecipesApi.Services.Interfaces
{
    public interface IIngredientService
    {
        public Task<IEnumerable<IngredientDTO>> GetIngredientsAsync();
        public Task<IngredientDTO> GetIngredientByIdAsync(int id);
        public Task<IngredientDTO> CreateIngredientAsync(CreateIngredientDTO createIngredientDTO, int userId);
        public Task<IngredientDTO> UpdateIngredientAsync(int id, UpdateIngredientDTO updateIngredientDTO, int userId);
        public Task DeleteIngredientAsync(int id, int userId);
    }
}
