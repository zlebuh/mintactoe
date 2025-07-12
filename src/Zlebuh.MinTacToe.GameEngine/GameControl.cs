using Zlebuh.MinTacToe.GameEngine.Exceptions;
using Zlebuh.MinTacToe.GameEngine.ModelExtensions;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine
{
    public static class GameControl
    {
        private static readonly Random random = new();
        public static Game Initialize(Rules rules)
        {
            Grid grid = [];
            for (int row = 0; row < rules.Rows; row++)
            {
                for (int col = 0; col < rules.Columns; col++)
                {
                    Coordinate coordinate = new(row, col);
                    Field field = new()
                    {
                        Player = null,
                        SurroundedByNotExplodedMines = 0,
                        IsMine = false,
                        Generated = false,
                        HasAllNeighboursGenerated = false
                    };
                    grid[coordinate] = field;
                }
            }

            Game game = new()
            {
                GameState = new()
                {
                    Grid = grid,
                    IsGameOver = false,
                    Changes = [],
                    PlayerOnTurn = Player.O,
                    Winner = null,
                    MovesPlayed = 0
                },
                Rules = rules
            };
            return game;
        }

        public static void MakeMove(Game game, Player player, Coordinate coordinate)
        {
            if (game.GameState.IsGameOver || !game.GameState.PlayerOnTurn.HasValue)
            {
                throw new GameIsOverException();
            }
            if (game.GameState.PlayerOnTurn.Value != player)
            {
                throw new NotYourTurnException(game.GameState.PlayerOnTurn.Value);
            }
            if (!coordinate.IsOnGrid(game))
            {
                throw new CoordinateOutOfGridException(coordinate, game.Rules.Rows, game.Rules.Columns);
            }

            Field field = game.GameState.Grid[coordinate];

            if (field.Player.HasValue)
            {
                throw new FieldOccupiedException(field.Player.Value, coordinate);
            }

            // Game state is not changed when exception is thrown. Game is not corrupted.

            if (!field.Generated)
            {
                field.IsMine = game.GameState.MovesPlayed >= game.Rules.NoMineMoves && random.NextDouble() < game.Rules.MineProbability;
                field.Generated = true;
            }

            if (!field.HasAllNeighboursGenerated)
            {
                foreach (Coordinate neighbourCoordinate in coordinate.AllNeighbors().Values)
                {
                    if (!neighbourCoordinate.IsOnGrid(game))
                    {
                        continue;
                    }

                    Field neighbour = game.GameState.Grid[neighbourCoordinate];
                    if (!neighbour.Generated)
                    {
                        neighbour.IsMine = random.NextDouble() < game.Rules.MineProbability;
                        neighbour.Generated = true;
                    }
                    if (neighbour.IsMine)
                    {
                        field.SurroundedByNotExplodedMines++;
                    }
                }
                field.HasAllNeighboursGenerated = true;
            }
            List<Coordinate> changedFieldCoordinates = [];
            if (field.IsMine)
            {
                changedFieldCoordinates.AddRange(game.ExplodeMine(player, coordinate));
            }
            changedFieldCoordinates.Add(coordinate);
            field.Player = player;



            bool playerWins = game.CheckPlayerWins(player, coordinate);
            bool isTie = game.CheckTie();
            bool gameOver = playerWins || isTie;
            game.GameState.IsGameOver = gameOver;
            game.GameState.Winner = playerWins ? player : null;
            game.GameState.PlayerOnTurn = gameOver ? null : player == Player.O ? Player.X : Player.O;
            game.GameState.Changes = changedFieldCoordinates;
            game.GameState.MovesPlayed++;
        }
    }
}
