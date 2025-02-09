namespace Zlebuh.MinTacToe.Model
{
    public class Game
    {
        public GameState GameState { get; internal set; } = null!;
        public Rules Rules { get; internal set; } = null!;
        public Game MakeCopy()
        {
            Game copy = new();
            Grid gridCopy = [];
            foreach (var kvp in GameState.Grid)
            {
                gridCopy[kvp.Key] = kvp.Value;
            }
            copy.GameState = new GameState
            {
                Grid = gridCopy,
                IsGameOver = GameState.IsGameOver,
                Winner = GameState.Winner,
                PlayerOnTurn = GameState.PlayerOnTurn,
                Changes = new List<Coordinate>(GameState.Changes),
                MovesPlayed = GameState.MovesPlayed
            };
            copy.Rules = Rules; // no need to create new instance
            return copy;
        }
    }
}
