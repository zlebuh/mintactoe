using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.GameModel;

public class Field
{
    [JsonInclude]
    public Player? Player { get; set; }
    [JsonInclude]
    public int SurroundedByNotExplodedMines { get; set; }
    [JsonInclude]
    public bool IsMine { get; set; }
    [JsonInclude]
    public bool Generated { get; set; }
    [JsonInclude]
    public bool HasAllNeighboursGenerated { get; set; }
}
