using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Auth;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.User;
using RecipesApi.Entities;
using System.Security.Claims;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
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

            return Ok(users);
        }

        // GET: api/User/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await GetUserDtoById(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST: api/User
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserDTO>> CreateUser(CreateUserDTO createUserDTO)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == createUserDTO.Username || u.Email == createUserDTO.Email);

            if (userExists)
            {
                return BadRequest("Użytkownik o podanej nazwie lub emailu już istnieje.");
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

            if (userDTO == null) return NotFound();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDTO);
        }

        // PUT: api/User/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, UpdateUserDTO updateUserDTO)
        {
            // Pobierz id użytkownika z tokena
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();
            if (int.Parse(userId) != id) return Forbid();

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Username = updateUserDTO.Username;
            user.Email = updateUserDTO.Email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDTO.Password);

            await _context.SaveChangesAsync();

            var userDTO = await GetUserDtoById(user.Id);

            if (userDTO == null) return NotFound();

            return Ok(userDTO);
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();
            if (int.Parse(userId) != id) return Forbid();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // [NIEPOTRZEBNE DZIĘKI USUAWNIU KASKADOWEMU]
            //// Usuń powiązane przepisy użytkownika
            //_context.Recipes.RemoveRange(user.Recipes);

            // Usuń użytkownika
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<UserDTO?> GetUserDtoById(int id)
        {
            return _context.Users
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
        }
    }
}
