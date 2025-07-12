using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.GameModel;

public class Game
{
    [JsonInclude]
    public GameState GameState { get; set; } = new();
    [JsonInclude]
    public Rules Rules { get; set; } = new();
    public Game MakeCopy()
    {
        return new()
        {
            GameState = new GameState
            {
                Grid = GameState.Grid.MakeCopy(),
                IsGameOver = GameState.IsGameOver,
                Winner = GameState.Winner,
                PlayerOnTurn = GameState.PlayerOnTurn,
                Changes = [.. GameState.Changes],
                MovesPlayed = GameState.MovesPlayed
            },
            Rules = Rules
        };
    }
}
