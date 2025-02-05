namespace Zlebuh.MinTacToe.Exceptions
{
    public class GameIsOverException : TicTacToeException
    {
        public GameIsOverException() : base("Game is over.")
        {
            
        }
    }
}
