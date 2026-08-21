namespace RecipesApi.Entities
{
    public class Step
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public int StepNumber { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }
}
