using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe
{
    public interface IAIPlayer
    {
        Coordinate MakeMove(Game game, Player player);
    }
}
