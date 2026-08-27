using Microsoft.AspNetCore.Mvc;
using RecipesApi.DTOs.Auth;
using RecipesApi.DTOs.User;
using RecipesApi.Services;


namespace RecipesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            var result = await _authService.Register(registerDTO);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ResultMessage);
            }

            return Ok(result.Data);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await _authService.Login(loginDTO);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.ResultMessage);
            }

            return Ok(result.Data);
        }
    }
}
