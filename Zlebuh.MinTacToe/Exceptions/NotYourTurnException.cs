using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Exceptions
{
    internal class NotYourTurnException : Exception
    {
        public NotYourTurnException(Player playerOnTurn) 
            : base($"{playerOnTurn} is on turn.")
        {
            PlayerOnTurn = playerOnTurn;
        }
        public Player PlayerOnTurn { get; }
    }
}
