namespace Zlebuh.MinTacToe.GameModel;

public readonly struct Coordinate(int row, int col)
{
    public int Row { get; } = row;
    public int Col { get; } = col;
}
