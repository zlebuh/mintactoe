namespace Zlebuh.MinTacToe.Model
{
    public class Dimension
    {
        internal Dimension(int rows, int columns)
        {
            rowMax = rows - 1;
            colMax = columns - 1;
        }        
        private static readonly int rowMin = 0;
        private readonly int rowMax;
        private static readonly int colMin = 0;
        private readonly int colMax;
        public bool CoordinateIsIn(Coordinate coordinate)
        {
            return coordinate.Row >= rowMin && coordinate.Row <= rowMax && coordinate.Col >= colMin && coordinate.Col <= colMax;
        }
    }
}
