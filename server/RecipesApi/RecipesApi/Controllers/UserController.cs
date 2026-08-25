using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.User;
using RecipesApi.Entities;

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
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Recipes)
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
                PasswordHash = createUserDTO.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDTO = await GetUserDtoById(user.Id);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDTO);
        }

        // PUT: api/User/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, UpdateUserDTO updateUserDTO)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Username = updateUserDTO.Username;
            user.Email = updateUserDTO.Email;
            user.PasswordHash = updateUserDTO.Password;

            await _context.SaveChangesAsync();

            var userDTO = await GetUserDtoById(user.Id);

            return Ok(userDTO);
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Recipes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Usuń powiązane przepisy użytkownika
            _context.Recipes.RemoveRange(user.Recipes);

            // Usuń użytkownika
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<UserDTO?> GetUserDtoById(int id)
        {
            return _context.Users
                .Include(u => u.Recipes)
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
