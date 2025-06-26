using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class FieldOccupiedException(Player player, Coordinate coordinate) 
        : MinTacToeException($"Field (0-based) [r{coordinate.Row}, " +
            $"c{coordinate.Col}] is occupied by player: {player}.")
    {
        public Coordinate OccupiedCoordinate { get; } = coordinate;
        public Player OccupyingPlayer { get; } = player;
    }
}