namespace Zlebuh.MinTacToe.GameModel;

public class GameState
{
    public Grid Grid { get; set; } = [];
    public bool IsGameOver { get; set; }
    public Player? Winner { get; set; }
    public Player? PlayerOnTurn { get; set; }
    public List<Coordinate> Changes { get; set; } = [];
    public int MovesPlayed { get; set; }
}
