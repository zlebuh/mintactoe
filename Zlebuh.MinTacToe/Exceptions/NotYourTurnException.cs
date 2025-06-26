using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class NotYourTurnException(Player playerOnTurn) 
        : MinTacToeException($"{playerOnTurn} is on turn.")
    {
        public Player PlayerOnTurn { get; } = playerOnTurn;
    }
}
