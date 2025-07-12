using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.ModelExtensions;

internal static class GameOverChecks
{
    internal static bool CheckPlayerWins(this Game game, Player player, Coordinate coordinate)
    {
        if (game.Rules.SeriesLength == 1)
        {
            return true;
        }

        Dictionary<int, int> masterDirectionToSum = new()
            {
                { 0, 1 }, { 1, 1 }, { 2, 1 }, { 3, 1 }
            };
        foreach (KeyValuePair<Direction, Coordinate> kvp in coordinate.AllNeighbors())
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
                if (!coor.IsOnGrid(game))
                {
                    break;
                }

                Field f = game.GameState.Grid[coor];
                if (!f.Generated)
                {
                    break;
                }

                if (f.IsMine)
                {
                    break;
                }

                if (f.Player == player)
                {
                    masterDirectionToSum[masterDirection]++;
                    if (masterDirectionToSum[masterDirection] == game.Rules.SeriesLength)
                    {
                        return true;
                    }
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
                    if (!field.Player.HasValue)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
