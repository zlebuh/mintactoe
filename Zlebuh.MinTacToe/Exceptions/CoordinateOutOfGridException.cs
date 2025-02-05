using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class CoordinateOutOfGridException : TicTacToeException
    {
        public CoordinateOutOfGridException(Coordinate coordinate, byte rows, byte columns)
        {
            Coordinate = coordinate;
            Rows = rows;
            Columns = columns;
        }

        public Coordinate Coordinate { get; }
        public byte Rows { get; }
        public byte Columns { get; }
    }
}
