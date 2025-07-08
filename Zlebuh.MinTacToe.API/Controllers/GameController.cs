using Microsoft.AspNetCore.Mvc;
using Zlebuh.MinTacToe.APIModel;
using Zlebuh.MinTacToe.API.Services;

namespace Zlebuh.MinTacToe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController(IDatabase db, IGameProxy gameProxy) : ControllerBase
    {
        private readonly IDatabase db = db;
        private readonly IGameProxy gameProxy = gameProxy;

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.HostUserId))
            {
                return BadRequest("Host user ID is required.");
            }
            if (!await db.UserExistsAsync(request.HostUserId))
            {
                return NotFound("Host user does not exist.");
            }
            try
            {
                var gameState = await gameProxy.CreateEmptySerializedGame();
                (string gameId, string hostToken, string visitorToken) = await db.CreateGame(request.HostUserId, gameState);
                return Ok(new GameResponse { GameId = gameId, HostToken = hostToken, VisitorToken = visitorToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("token")]        
        public async Task<IActionResult> IsTokenValidForAGame([FromBody] TokenValidityRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GameId))
            {
                return BadRequest("Game ID is required.");
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest("User ID is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest("Token is required.");
            }

            (string hostToken, string visitorToken, string hostUserId, string? visitorUserId) = 
                await db.GetGameDetails(request.GameId);

            if (hostToken == request.Token)
            {
                return Ok(new TokenValidityResponse { IsValid = hostUserId == request.UserId });
            }
            else if (visitorToken == request.Token)
            {
                if (string.IsNullOrEmpty(visitorUserId))
                {
                    await db.SetVisitorUserId(request.GameId, request.UserId);
                    return Ok(new TokenValidityResponse { IsValid = true });
                }
                return Ok(new TokenValidityResponse { IsValid = visitorUserId == request.UserId });
            }
            else
            {
                return Ok(new TokenValidityResponse { IsValid = false });
            }
        }
    }
}
