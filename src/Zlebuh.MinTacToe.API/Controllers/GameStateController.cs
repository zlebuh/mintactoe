using Microsoft.AspNetCore.Mvc;
using Zlebuh.MinTacToe.API.Services;
using Zlebuh.MinTacToe.APIModel;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameStateController(IDatabase db, IGameProxy game, ILogger<GameStateController> logger) : ControllerBase
    {
        private readonly IDatabase db = db;
        private readonly IGameProxy game = game;
        private readonly ILogger<GameStateController> logger = logger;

        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGameState(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest("Game ID is required.");
            }
            string gameState = await db.GetGameState(gameId);
            return string.IsNullOrEmpty(gameState)
                ? NotFound("Game not found or has no state.")
                : Ok(new GameStateResponse { GameState = gameState, GameId = gameId });
        }

        [HttpPost]
        public async Task<IActionResult> MakeMove([FromBody] MakeMoveRequest request)
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

            Player player;

            if (hostToken == request.Token && hostUserId == request.UserId)
            {
                player = Player.O;
            }
            else if (visitorToken == request.Token && visitorUserId == request.UserId)
            {
                player = Player.X;
            }
            else
            {
                return Unauthorized("Invalid token or user ID for the game.");
            }


            string gameState = await db.GetGameState(request.GameId);
            Coordinate coordinate = new(request.CoordinateRow, request.CoordinateCol);
            logger.LogInformation($"Making a move {player} -> [{coordinate.Row},{coordinate.Col}]");
            (int errorCode, string message, string? newGameState) = await game.MakeAMove(gameState, coordinate, player);
            if (!string.IsNullOrEmpty(newGameState))
            {
                await db.UpdateGameState(request.GameId, newGameState);
            }

            return Ok(new MakeMoveResponse
            {
                Message = message,
                ErrorCode = errorCode
            });
        }
    }
}
