using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace RecipesApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Użytkownik nie jest zalogowany");
            }

            return int.Parse(userIdClaim.Value);
        }
    }
}
