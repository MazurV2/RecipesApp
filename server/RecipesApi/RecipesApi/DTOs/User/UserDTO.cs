using RecipesApi.DTOs.Recipe;

namespace RecipesApi.DTOs.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RecipeDTO> Recipes { get; set; } = new List<RecipeDTO>();
    }
}
