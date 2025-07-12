using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.GameEngine.Model
{
    public class GameState
    {
        [JsonInclude]
        public Grid Grid { get; internal set; } = [];
        [JsonInclude]
        public bool IsGameOver { get; internal set; }
        [JsonInclude]
        public Player? Winner { get; internal set; }
        [JsonInclude]
        public Player? PlayerOnTurn { get; internal set; }
        [JsonInclude]
        public List<Coordinate> Changes { get; internal set; } = [];
        [JsonInclude]
        public int MovesPlayed { get; internal set; }
    }
}
