
using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.Model
{
    public readonly struct Coordinate(int row, int col)
    {
        public int Row { get; } = row;
        public int Col { get; } = col;
        
        [JsonIgnore]
        public Dictionary<Direction, Coordinate> Neighbours => GetNeighbors();
        public bool IsOnGrid(Game game)
        {
            return game.Rules.Rows > Row && Row >= 0 && game.Rules.Columns > Col && Col >= 0;
        }

        private Dictionary<Direction, Coordinate> GetNeighbors()
        {
            return new Dictionary<Direction, Coordinate>()
            {
                { Direction.Up, new Coordinate(Row - 1, Col) },
                { Direction.Down, new Coordinate(Row + 1, Col) },
                { Direction.Left, new Coordinate(Row, Col - 1) },
                { Direction.Right, new Coordinate(Row, Col + 1) },
                { Direction.UpLeft, new Coordinate(Row - 1, Col - 1) },
                { Direction.UpRight, new Coordinate(Row - 1, Col + 1) },
                { Direction.DownRight, new Coordinate(Row + 1, Col + 1) },
                { Direction.DownLeft, new Coordinate(Row + 1, Col - 1) }
            };
        }
    }

    public enum Direction
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
}
