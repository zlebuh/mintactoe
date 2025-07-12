using Microsoft.AspNetCore.Mvc;
using Zlebuh.MinTacToe.APIModel;
using Zlebuh.MinTacToe.API.Services;

namespace Zlebuh.MinTacToe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IDatabase db) : ControllerBase
    {
        private readonly IDatabase db = db;

        [HttpPost]
        public async Task<IActionResult> CheckUserId([FromBody] UserRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                if (await db.UserExistsAsync(request.UserId))
                {
                    return Ok(new UserResponse() { UserId = request.UserId });
                }
            }

            try
            {
                var userId = await db.CreateUserAsync();
                return Ok(new UserResponse() { UserId = userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
