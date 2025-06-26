namespace Zlebuh.MinTacToe.Model
{
    public class GameState
    {
        public Grid Grid { get; internal set; } = [];
        public bool IsGameOver { get; internal set; }
        public Player? Winner { get; internal set; }
        public Player? PlayerOnTurn { get; internal set; }
        public List<Coordinate> Changes { get; internal set; } = [];
        public int MovesPlayed { get; internal set; }
    }
}
