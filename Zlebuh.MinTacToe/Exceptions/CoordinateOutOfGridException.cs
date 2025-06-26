using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class CoordinateOutOfGridException(Coordinate coordinate, byte rows, byte columns) : MinTacToeException
    {
        public Coordinate Coordinate { get; } = coordinate;
        public byte Rows { get; } = rows;
        public byte Columns { get; } = columns;
    }
}
