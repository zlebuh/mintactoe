using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine;

public static class GameControl
{
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
}
