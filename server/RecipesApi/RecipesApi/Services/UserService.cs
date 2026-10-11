using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.User;
using RecipesApi.Entities;
using RecipesApi.Services.Interfaces;

namespace RecipesApi.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        
        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<IEnumerable<UserDTO>> GetUsersAsync(int userId)
        {
            return await _context.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Recipes = u.Recipes.Select(r => new RecipeDTO
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description
                    }).ToList()
                }).ToListAsync();
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<UserDTO> GetUserByIdAsync(int id, int userId)
        {
            return await GetUserDtoById(id);
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDTO, int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == createUserDTO.Username.ToLower() || u.Email.ToLower() == createUserDTO.Email.ToLower());

            if (userExists)
            {
                throw new ConflictException("Użytkownik o podanej nazwie lub emailu już istnieje");
            }

            var user = new User
            {
                Username = createUserDTO.Username,
                Email = createUserDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDTO.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDTO = await GetUserDtoById(user.Id);            
            return userDTO;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO, int userId)
        {
            if (userId == null || userId != id)
            {
                throw new UnauthorizedAccessException("Nie masz uprawnień do zaktualizowania tego użytkownika");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono użytkownika o ID {id}");
            }

            if (!BCrypt.Net.BCrypt.Verify(updateUserDTO.OldPassword, user.PasswordHash))
            {
                throw new ConflictException("Niepoprawne stare hasło.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == updateUserDTO.Username.ToLower() || u.Email.ToLower() == updateUserDTO.Email.ToLower());
            if (userExists)
            {
                throw new ConflictException("Użytkownik o podanej nazwie lub emailu już istnieje.");
            }

            user.Username = updateUserDTO.Username;
            user.Email = updateUserDTO.Email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDTO.NewPassword);

            await _context.SaveChangesAsync();

            var userDTO = await GetUserDtoById(user.Id);
            return userDTO;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task DeleteUserAsync(int id, int userId)
        {
            if (userId == null || userId != id)
            {
                throw new UnauthorizedAccessException("Nie masz uprawnień do usunięcia tego użytkownika");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono użytkownika o ID {id}");
            }

            // Usuń użytkownika
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private async Task<UserDTO> GetUserDtoById(int id)
        {
            var userDto = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Recipes = u.Recipes.Select(r => new RecipeDTO
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (userDto == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono użytkownika o ID {id}");
            }

            return userDto;
        }
    }
}
