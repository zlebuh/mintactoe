using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Services
{
    public interface IGameControl
    {
        MoveOutcome PlaceMove(Player player, Coordinate coordinate);
        Grid Grid { get; }
        Player PlayerOnMove { get; }
    }
}
