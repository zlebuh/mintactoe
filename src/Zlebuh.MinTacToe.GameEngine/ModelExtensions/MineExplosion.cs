using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.ModelExtensions;

internal static class MineExplosion
{
    public static HashSet<Coordinate> ExplodeMine(this Game game, Player player, Coordinate mineCoordinate)
    {
        int minePower = game.Rules.MinePower;
        Grid grid = game.GameState.Grid;
        HashSet<Coordinate> coordinatesAffected = [];
        for (int i = mineCoordinate.Row - minePower; i <= mineCoordinate.Row + minePower; i++)
        {
            for (int j = mineCoordinate.Col - minePower; j <= mineCoordinate.Col + minePower; j++)
            {
                Coordinate c = new(i, j);
                if (!c.IsOnGrid(game))
                {
                    continue;
                }

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
}
