namespace Zlebuh.MinTacToe.Model
{
    public readonly struct Coordinate
    {
        public Coordinate(int row, int col)
        {
            Row = row;
            Col = col;
        }
        public int Row { get; }
        public int Col { get; }
    }
}
