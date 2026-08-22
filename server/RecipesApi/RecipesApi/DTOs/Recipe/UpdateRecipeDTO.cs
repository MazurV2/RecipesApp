using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;
using RecipesApi.Entities;

namespace RecipesApi.DTOs.Recipe
{
    public class UpdateRecipeDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<UpdateRecipeIngredientDTO> RecipeIngredients { get; set; } = new List<UpdateRecipeIngredientDTO>();
        public ICollection<UpdateStepDTO> Steps { get; set; } = new List<UpdateStepDTO>();
        public int Calories { get; set; } = 0;
        public DifficultyLevel Difficulty { get; set; }
    }
}
