using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.GameModel;

public class Game
{
    [JsonInclude]
    public GameState GameState { get; set; } = new();
    [JsonInclude]
    public Rules Rules { get; set; } = new();
}
