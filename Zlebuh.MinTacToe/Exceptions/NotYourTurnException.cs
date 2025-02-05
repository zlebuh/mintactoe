using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    public class NotYourTurnException : TicTacToeException
    {
        public NotYourTurnException(Player playerOnTurn) 
            : base($"{playerOnTurn} is on turn.")
        {
            PlayerOnTurn = playerOnTurn;
        }
        public Player PlayerOnTurn { get; }
    }
}
