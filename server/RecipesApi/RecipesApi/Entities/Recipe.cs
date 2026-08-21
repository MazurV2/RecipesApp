namespace RecipesApi.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<Step> Steps { get; set; } = new List<Step>();
        public int Calories { get; set; } = 0;
        public DifficultyLevel Difficulty { get; set; }
    }
}
