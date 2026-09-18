using RecipesApi.Entities;

namespace RecipesApi.Services.Interfaces
{
    public interface IRecipeDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; }
        public int Calories { get; set; }
        public DifficultyLevel Difficulty { get; set; }
    }
}
