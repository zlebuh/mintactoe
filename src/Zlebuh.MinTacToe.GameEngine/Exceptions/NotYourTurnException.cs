using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.Exceptions
{
    public class NotYourTurnException(Player playerOnTurn)
        : MinTacToeException($"{playerOnTurn} is on turn.")
    {
        public Player PlayerOnTurn { get; } = playerOnTurn;
    }
}
