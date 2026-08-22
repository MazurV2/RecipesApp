using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;
using RecipesApi.Entities;

namespace RecipesApi.DTOs.Recipe
{
    public class CreateRecipeDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<CreateRecipeIngredientDTO> RecipeIngredients { get; set; } = new List<CreateRecipeIngredientDTO>();
        public ICollection<CreateStepDTO> Steps { get; set; } = new List<CreateStepDTO>();
        public int Calories { get; set; } = 0;
        public DifficultyLevel Difficulty { get; set; }
    }
}
