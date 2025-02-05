namespace Zlebuh.MinTacToe.Model
{
    public class Game
    {
        public GameState GameState { get; internal set; } = null!;
        public Rules Rules { get; internal set; } = null!;
    }
}
