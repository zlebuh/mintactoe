namespace Zlebuh.MinTacToe.GameModel;

public class Rules
{
    public byte Rows { get; set; } = 20;
    public byte Columns { get; set; } = 20;
    public byte SeriesLength { get; set; } = 5;
    public byte NoMineMoves { get; set; } = 6;
    public byte MinePower { get; set; } = 1;
    public double MineProbability { get; set; } = 0.1;
}
