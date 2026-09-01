using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;

namespace RecipesApi.DTOs.Recipe
{
    public class RecipeDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<RecipeIngredientDTO> RecipeIngredients { get; set; } = new List<RecipeIngredientDTO>();
        public List<StepDTO> Steps { get; set; } = new List<StepDTO>();
        public int Calories { get; set; } = 0;
        public string Difficulty { get; set; } = string.Empty;
    }
}
