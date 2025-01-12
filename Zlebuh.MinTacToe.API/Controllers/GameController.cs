using Microsoft.AspNetCore.Mvc;
using Zlebuh.MinTacToe.API.Services;
using Zlebuh.MinTacToe.Model;
using Zlebuh.MinTacToe.Services;

namespace Zlebuh.MinTacToe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> logger;
        private readonly IGameFactory gameFactory;
        private readonly IGamesPersister gamePersister;

        public GameController(ILogger<GameController> logger, IGameFactory gameFactory, IGamesPersister gamePersister)
        {
            this.logger = logger;
            this.gameFactory = gameFactory;
            this.gamePersister = gamePersister;
        }

        [HttpGet(Name = "CreateGame")]
        public GameMetaData Create()
        {
            Rules rules = new();
            IGameControl gameControl = gameFactory.Create(rules);
            string gameId = gamePersister.AddGame(gameControl);
            logger.LogInformation("Game with id {gameId} is on.", gameId);
            return new()
            {
                GameId = gameId,
                RowsCount = rules.Rows,
                ColsCount = rules.Columns
            };
        }

        [HttpPost, Route("PlaceMove")]
        public MoveOutcome PlaceMove(PlaceMoveArguments arguments)
        {
            IGameControl gameControl = gamePersister.GetGame(arguments.GameId) ?? throw new Exception();
            if (gameControl.PlayerOnMove != arguments.Player)
            {
                throw new Exception();
            }

            MoveOutcome moveOutcome = gameControl.PlaceMove(arguments.Player, new(arguments.Row, arguments.Col));
            if (moveOutcome != MoveOutcome.GameIsOn)
            {
                logger.LogInformation("Game with id {gameId} is finished.", arguments.GameId);
                gamePersister.RemoveGame(arguments.GameId);
            }

            return moveOutcome;
        }

        [HttpPost, Route("Field")]
        public Field? GetField(FieldArguments arguments)
        {
            IGameControl gameControl = gamePersister.GetGame(arguments.GameId) ?? throw new Exception();
            if (gameControl.Grid.FieldGenerated(new(arguments.Row, arguments.Col))) return gameControl.Grid[new(arguments.Row, arguments.Col)];
            return null;
        }

        [HttpGet("{gameId}")]
        public Player GetPlayerOnMove(string gameId) 
        {
            IGameControl gameControl = gamePersister.GetGame(gameId) ?? throw new Exception();
            return gameControl.PlayerOnMove;
        }

        public class PlayerArguments
        {
            public string GameId { get; set; } = null!;
        }

        public class FieldArguments
        {
            public string GameId { get; set; } = null!;
            public int Row { get; set; }
            public int Col { get; set; }
            //public Coordinate Coordinate  { get; set; }
        }

        public class GameMetaData
        {
            public string GameId { get; set; } = null!;
            public int RowsCount { get; set; }
            public int ColsCount { get; set; }
        }

        public class PlaceMoveArguments
        {
            public string GameId { get; set; } = null!;
            public Player Player { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
            //public Coordinate Coordinate { get; set; }
        }
    }
}