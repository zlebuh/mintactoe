namespace Zlebuh.MinTacToe.Exceptions
{
    public abstract class TicTacToeException : Exception
    {
        protected TicTacToeException(string message) : base(message)
        {
            
        }
        protected TicTacToeException() : base()
        {
            
        }
    }
}
