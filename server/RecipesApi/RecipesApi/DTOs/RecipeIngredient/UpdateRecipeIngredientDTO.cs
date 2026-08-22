namespace RecipesApi.DTOs.RecipeIngredient
{
    public class UpdateRecipeIngredientDTO
    {
        public int IngredientId { get; set; }
        public decimal Amount { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
    }
}
