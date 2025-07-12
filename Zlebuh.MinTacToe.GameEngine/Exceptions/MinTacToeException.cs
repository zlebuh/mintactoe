namespace Zlebuh.MinTacToe.GameEngine.Exceptions
{
    public abstract class MinTacToeException : Exception
    {
        protected MinTacToeException(string message) : base(message) { }
        protected MinTacToeException() : base() { }
    }
}
 