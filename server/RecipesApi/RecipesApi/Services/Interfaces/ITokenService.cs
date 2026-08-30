using RecipesApi.Entities;

namespace RecipesApi.Services.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiryDate) GenerateJwtToken(User user);
    }
}
