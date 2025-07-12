namespace Zlebuh.MinTacToe.GameModel;

public class Game
{
    public GameState GameState { get; set; } = new();
    public Rules Rules { get; set; } = new();
}
