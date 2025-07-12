
using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.GameModel;

public readonly struct Coordinate(int row, int col)
{
    [JsonInclude]
    public int Row { get; } = row;
    [JsonInclude]
    public int Col { get; } = col;
}
