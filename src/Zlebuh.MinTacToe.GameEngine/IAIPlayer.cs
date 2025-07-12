using Zlebuh.MinTacToe.GameEngine.Model;

namespace Zlebuh.MinTacToe.GameEngine
{
    public interface IAIPlayer
    {
        Coordinate MakeMove(Game game, Player player);
    }
}
