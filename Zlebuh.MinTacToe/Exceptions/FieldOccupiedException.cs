using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class FieldOccupiedException : TicTacToeException
    {
        public FieldOccupiedException(Player player, Coordinate coordinate) 
            : base($"Field (0-based) [r{coordinate.Row}, c{coordinate.Col}] is occupied by player: {player}.")
        {
            OccupiedCoordinate = coordinate;
            OccupyingPlayer = player;
        }
        public Coordinate OccupiedCoordinate { get; }
        public Player OccupyingPlayer { get; }
    }
}
