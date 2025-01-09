using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class FieldOccupiedException : Exception
    {
        public FieldOccupiedException(Player player, Coordinate coordinate) 
            : base($"Field is taken. {player} [r{coordinate.Row}, c{coordinate.Col}]")
        {
            
        }
    }
}
