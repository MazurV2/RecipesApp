using RecipesApi.DTOs.Recipe;
using RecipesApi.Pagination;

namespace RecipesApi.Services.Interfaces
{
    public interface IRecipeService
    {
        Task<PagedResults<RecipeDTO>> GetRecipesAsync(RecipeQueryDTO queryDTO);
        Task<RecipeDTO> GetRecipeByIdAsync(int id);
        Task<RecipeDTO> CreateRecipeAsync(CreateRecipeDTO createRecipeDTO, int userId);
        Task<RecipeDTO> UpdateRecipeAsync(int id, UpdateRecipeDTO updateRecipeDTO, int userId);
        Task DeleteRecipeAsync(int id, int userId);
    }
}
