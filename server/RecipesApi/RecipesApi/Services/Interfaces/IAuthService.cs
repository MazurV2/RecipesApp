using RecipesApi.DTOs.Auth;
using RecipesApi.DTOs.User;

namespace RecipesApi.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<ServiceResult<UserDTO>> Register(RegisterDTO registerDTO);
        public Task<ServiceResult<AuthResponseDTO>> Login(LoginDTO loginDTO);
    }
}
