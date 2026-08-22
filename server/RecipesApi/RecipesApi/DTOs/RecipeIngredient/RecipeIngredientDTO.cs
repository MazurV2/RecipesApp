namespace RecipesApi.DTOs.RecipeIngredient
{
    public class RecipeIngredientDTO
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
    }
}
