using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.AIPlayers
{
    public class Random : IAIPlayer
    {
        public int MaxDistance { get; set; }
        private readonly System.Random random = new();
        public Coordinate MakeMove(Game game, Player player)
        {
            List<Coordinate> unoccupied = GetUnoccupiedCoordinates(game.GameState.Grid).ToList();
            if (unoccupied.Count == 0)
            {
                throw new InvalidOperationException("No unoccupied fields.");
            }
            int index = random.Next(unoccupied.Count);
            return unoccupied[index];
        }

        private HashSet<Coordinate> GetUnoccupiedCoordinates(Grid grid)
        {
            return grid.Where(kvp => !kvp.Value.Player.HasValue).Select(kvp => kvp.Key).ToHashSet();
        }
    }
}
