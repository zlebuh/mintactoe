using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.Exceptions
{
    public class CoordinateOutOfGridException(Coordinate coordinate, byte rows, byte columns) : MinTacToeException
    {
        public Coordinate Coordinate { get; } = coordinate;
        public byte Rows { get; } = rows;
        public byte Columns { get; } = columns;
    }
}
