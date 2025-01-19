using System.Runtime.CompilerServices;
using Zlebuh.MinTacToe.Exceptions;
using Zlebuh.MinTacToe.Model;

[assembly: InternalsVisibleTo("Zlebuh.MinTacToe.Tests")]

namespace Zlebuh.MinTacToe
{
    public static class GameControl
    {
        private readonly static (int, int)[] neighbours = new (int, int)[]
        {
            (1, 0), (0, 1), (-1, 0), (0, -1), (-1, 1), (1, 1), (1, -1), (-1, -1)
        };

        private readonly static Random random = new();
        public static Game Initialize(Rules rules)
        {
            Grid grid = new();
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
                        ErasedByExplodedMine = false,
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
                    Changes = new(),
                    PlayerOnTurn = Player.O,
                    Winner = null
                },
                Rules = rules,
                MovesPlayed = 0
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
                field.IsMine = game.MovesPlayed >= game.Rules.NoMineMoves && random.NextDouble() < game.Rules.MineProbability;
                field.Generated = true;
            }

            if (!field.HasAllNeighboursGenerated)
            {
                foreach ((int r, int c) in neighbours)
                {
                    Coordinate neighbourCoordinate = new(coordinate.Row + r, coordinate.Col + c);
                    if (!neighbourCoordinate.IsOnGrid(game)) continue;
                    Field neighbour = game.GameState.Grid[neighbourCoordinate];
                    if (!neighbour.Generated)
                    {
                        neighbour.IsMine = random.NextDouble() < game.Rules.MineProbability;
                        neighbour.Generated = true;
                    }
                    if (neighbour.IsMine) field.SurroundedByNotExplodedMines++;
                }
                field.HasAllNeighboursGenerated = true;
            }
            List<Coordinate> changedFieldCoordinates = new();
            if (field.IsMine)
            {
                changedFieldCoordinates.AddRange(game.ExplodeMine(player, coordinate));
            }

            field.Player = player;
            field.ErasedByExplodedMine = false; // no more
            changedFieldCoordinates.Add(coordinate);

            bool playerWins = game.CheckPlayerWins(player, coordinate);
            bool isTie = game.CheckTie();
            bool gameOver = playerWins || isTie;
            game.GameState.IsGameOver = gameOver;
            game.GameState.Winner = playerWins ? player : null;
            game.GameState.PlayerOnTurn = gameOver ? null : player == Player.O ? Player.X : Player.O;
            game.GameState.Changes = changedFieldCoordinates;
            game.MovesPlayed++;
        }

        internal static bool IsOnGrid(this Coordinate coordinate, Game game)
        {
            int minIndex = 0;
            int maxRowIndex = game.Rules.Rows - 1;
            int maxColIndex = game.Rules.Columns - 1;
            return coordinate.Row >= minIndex && coordinate.Row <= maxRowIndex && coordinate.Col >= minIndex && coordinate.Col <= maxColIndex;
        }

        internal static List<Coordinate> ExplodeMine(this Game game, Player player, Coordinate coordinate)
        {
            int minePower = game.Rules.MinePower;
            Grid grid = game.GameState.Grid;
            List<Coordinate> coordinatesAffected = new();
            for (int i = coordinate.Row - minePower; i <= coordinate.Row + minePower; i++)
            {
                for (int j = coordinate.Col - minePower; j <= coordinate.Col + minePower; j++)
                {
                    Coordinate c = new(i, j);
                    if (c.Row == coordinate.Row && c.Col == coordinate.Col) continue;
                    if (c.IsOnGrid(game))
                    {
                        Field f = grid[c];
                        if (f.Player == player)
                        {
                            f.Player = null;
                            f.ErasedByExplodedMine = true;
                        }
                        coordinatesAffected.Add(c);
                        f.SurroundedByNotExplodedMines--;
                    }
                }
            }
            return coordinatesAffected;
        }

        internal static bool CheckPlayerWins(this Game game, Player player, Coordinate coordinate)
        {
            if (game.Rules.SeriesLength == 1) return true;
            foreach ((int r, int c) in neighbours)
            {
                int rc = coordinate.Row;
                int cc = coordinate.Col;
                int counter = 1;
                do
                {
                    rc += r;
                    cc += c;
                    Coordinate coor = new(rc, cc);
                    if (!coor.IsOnGrid(game)) break;
                    Field f = game.GameState.Grid[coor];
                    if (!f.Generated) break;
                    if (f.Player == player)
                    {
                        counter++;
                        if (counter == game.Rules.SeriesLength) return true;
                    }
                    else
                    {
                        break;
                    }
                } while (true);
            }
            return false;
        }

        internal static bool CheckTie(this Game game)
        {
            for (int row = 0; row < game.Rules.Rows; row++)
            {
                for (int col = 0; col < game.Rules.Columns; col++)
                {
                    Coordinate coordinate = new(row, col);
                    Field field = game.GameState.Grid[coordinate];
                    if (!field.IsMine)
                    {
                        if (!field.Player.HasValue) return false;
                    }
                }
            }
            return true;
        }
    }
}
