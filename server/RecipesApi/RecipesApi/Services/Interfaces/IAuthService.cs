using RecipesApi.DTOs.Auth;
using RecipesApi.DTOs.User;

namespace RecipesApi.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<UserDTO> Register(RegisterDTO registerDTO);
        public Task<AuthResponseDTO> Login(LoginDTO loginDTO);
    }
}
