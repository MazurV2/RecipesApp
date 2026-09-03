namespace RecipesApi.DTOs.Recipe
{
    public class RecipeQueryDTO
    {
        public string? SearchTerm { get; set; }

        public int? MinCalories { get; set; }
        public int? MaxCalories { get; set; }

        public int? MinDifficulty { get; set; }
        public int? MaxDifficulty { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}
