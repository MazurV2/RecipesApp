using RecipesApi.DTOs.User;

namespace RecipesApi.Services.Interfaces
{
    public interface IUserService
    {
        public Task<IEnumerable<UserDTO>> GetUsersAsync(int userId);
        public Task<UserDTO> GetUserByIdAsync(int id, int userId);
        public Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDTO, int userId);
        public Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO, int userId);
        public Task DeleteUserAsync(int id, int userId);
    }
}
