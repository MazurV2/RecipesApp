using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.User;
using RecipesApi.Entities;
using RecipesApi.Services.Interfaces;
using System.Security.Claims;

namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var userDTOs = await _userService.GetUsersAsync(GetUserIdFromClaims());

            return Ok(userDTOs);
        }

        // GET: api/User/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var userId = GetUserIdFromClaims();

            var userDTO = await _userService.GetUserByIdAsync(id, userId);

            return Ok(userDTO);
        }

        // POST: api/User
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserDTO>> CreateUser(CreateUserDTO createUserDTO)
        {
            var userId = GetUserIdFromClaims();

            var userDTO = await _userService.CreateUserAsync(createUserDTO, userId);

            return CreatedAtAction(nameof(GetUser), new { id = userDTO.Id }, userDTO);
        }

        // PUT: api/User/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, UpdateUserDTO updateUserDTO)
        {
            var userDTO = await _userService.UpdateUserAsync(id, updateUserDTO, GetUserIdFromClaims());

            return Ok(userDTO);
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id, GetUserIdFromClaims());

            return NoContent();
        }
    }
}
