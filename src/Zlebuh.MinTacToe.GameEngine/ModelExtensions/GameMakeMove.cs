using Zlebuh.MinTacToe.GameEngine.Exceptions;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.ModelExtensions;


public static class GameMakeMove
{
    public static void MakeMove(this Game game, Player player, Coordinate coordinate)
    {
        game.CheckArgumentsValidity(player, coordinate, out Field field);

        game.GenerateFieldOrNeighbors(coordinate, field);

        game.ApplyMoveToAGrid(player, coordinate, field);

        game.CheckGameOver(player, coordinate);

        game.GameState.MovesPlayed++;
    }

    internal static void CheckArgumentsValidity(this Game game, Player player, Coordinate coordinate, out Field field)
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

        field = game.GameState.Grid[coordinate];

        if (field.Player.HasValue)
        {
            throw new FieldOccupiedException(field.Player.Value, coordinate);
        }
    }

    internal static void GenerateFieldOrNeighbors(this Game game, Coordinate coordinate, Field field)
    {
        field.GenerateSafely(game);

        if (!field.HasAllNeighboursGenerated)
        {
            foreach (Coordinate neighbourCoordinate in coordinate.AllNeighbors().Values)
            {
                if (!neighbourCoordinate.IsOnGrid(game))
                {
                    continue;
                }

                Field neighbour = game.GameState.Grid[neighbourCoordinate];
                neighbour.Generate(game);
                if (neighbour.IsMine)
                {
                    field.SurroundedByNotExplodedMines++;
                }
            }
            field.HasAllNeighboursGenerated = true;
        }
    }

    internal static void ApplyMoveToAGrid(this Game game, Player player, Coordinate coordinate, Field field)
    {
        game.GameState.Changes.Clear();
        if (field.IsMine)
        {
            game.GameState.Changes.AddRange(game.ExplodeMine(player, coordinate));
        }
        game.GameState.Changes.Add(coordinate);
        field.Player = player;
    }

    internal static void CheckGameOver(this Game game, Player player, Coordinate coordinate)
    {
        bool playerWins = game.CheckPlayerWins(player, coordinate);
        bool isTie = game.CheckTie();
        bool gameOver = playerWins || isTie;
        game.GameState.IsGameOver = gameOver;
        game.GameState.Winner = playerWins ? player : null;
        game.GameState.PlayerOnTurn = gameOver ? null : player == Player.O ? Player.X : Player.O;
    }
}