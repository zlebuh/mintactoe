using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.GameModel;

public class GameState
{
    [JsonInclude]
    public Grid Grid { get; set; } = [];
    [JsonInclude]
    public bool IsGameOver { get; set; }
    [JsonInclude]
    public Player? Winner { get; set; }
    [JsonInclude]
    public Player? PlayerOnTurn { get; set; }
    [JsonInclude]
    public List<Coordinate> Changes { get; set; } = [];
    [JsonInclude]
    public int MovesPlayed { get; set; }
}
