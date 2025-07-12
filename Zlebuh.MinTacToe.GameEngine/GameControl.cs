using System.Runtime.CompilerServices;
using Zlebuh.MinTacToe.GameEngine.Exceptions;
using Zlebuh.MinTacToe.GameEngine.Model;

[assembly: InternalsVisibleTo("Zlebuh.MinTacToe.Tests")]

namespace Zlebuh.MinTacToe.GameEngine
{
    public static class GameControl
    {
        private readonly static Random random = new();
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
                foreach (var neighbourCoordinate in coordinate.Neighbours.Values)
                {
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

        internal static HashSet<Coordinate> ExplodeMine(this Game game, Player player, Coordinate mineCoordinate)
        {
            int minePower = game.Rules.MinePower;
            Grid grid = game.GameState.Grid;
            HashSet<Coordinate> coordinatesAffected = [];
            for (int i = mineCoordinate.Row - minePower; i <= mineCoordinate.Row + minePower; i++)
            {
                for (int j = mineCoordinate.Col - minePower; j <= mineCoordinate.Col + minePower; j++)
                {
                    Coordinate c = new(i, j);
                    if (!c.IsOnGrid(game)) continue;
                    
                    Field f = grid[c];
                    
                    if (c.Row == mineCoordinate.Row && c.Col == mineCoordinate.Col)
                    {
                        continue;
                    }

                    if (!f.IsMine)
                    {
                        if (f.Player.HasValue)
                        {
                            coordinatesAffected.Add(c);
                            if (f.Player == player)
                            {
                                f.Player = null;
                            }
                        } 
                    }

                    f.SurroundedByNotExplodedMines--;
                }
            }
            return coordinatesAffected;
        }

        internal static bool CheckPlayerWins(this Game game, Player player, Coordinate coordinate)
        {
            if (game.Rules.SeriesLength == 1) return true;
            Dictionary<int, int> masterDirectionToSum = new()
            {
                { 0, 1 }, { 1, 1 }, { 2, 1 }, { 3, 1 }
            };
            foreach (var kvp in coordinate.Neighbours)
            {
                Direction direction = kvp.Key;
                (int r, int c) = (kvp.Value.Row - coordinate.Row, kvp.Value.Col - coordinate.Col);
                int masterDirection = (int)direction % 4;
                int rc = coordinate.Row;
                int cc = coordinate.Col;
                do
                {
                    rc += r;
                    cc += c;
                    Coordinate coor = new(rc, cc);
                    if (!coor.IsOnGrid(game)) break;
                    Field f = game.GameState.Grid[coor];
                    if (!f.Generated) break;
                    if (f.IsMine) break;
                    if (f.Player == player)
                    {
                        masterDirectionToSum[masterDirection]++;
                        if (masterDirectionToSum[masterDirection] == game.Rules.SeriesLength) return true;
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
