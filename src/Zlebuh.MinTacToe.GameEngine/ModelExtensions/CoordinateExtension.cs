using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.ModelExtensions
{
    internal enum Direction
    {
        Up = 0,
        Left = 1,
        UpLeft = 2,
        UpRight = 3,
        Down = 4,
        Right = 5,
        DownRight = 6,
        DownLeft = 7
    }

    internal static class CoordinateExtension
    {
        public static Dictionary<Direction, Coordinate> AllNeighbors(this Coordinate coordinate)
        {
            return new Dictionary<Direction, Coordinate>()
            {
                { Direction.Up, new Coordinate(coordinate.Row - 1, coordinate.Col) },
                { Direction.Down, new Coordinate(coordinate.Row + 1, coordinate.Col) },
                { Direction.Left, new Coordinate(coordinate.Row, coordinate.Col - 1) },
                { Direction.Right, new Coordinate(coordinate.Row, coordinate.Col + 1) },
                { Direction.UpLeft, new Coordinate(coordinate.Row - 1, coordinate.Col - 1) },
                { Direction.UpRight, new Coordinate(coordinate.Row - 1, coordinate.Col + 1) },
                { Direction.DownRight, new Coordinate(coordinate.Row + 1, coordinate.Col + 1) },
                { Direction.DownLeft, new Coordinate(coordinate.Row + 1, coordinate.Col - 1) }
            };
        }

        public static bool IsOnGrid(this Coordinate coordinate, Game game)
        {
            return game.Rules.Rows > coordinate.Row && coordinate.Row >= 0
                && game.Rules.Columns > coordinate.Col && coordinate.Col >= 0;

        }
    }
}
