namespace Zlebuh.MinTacToe.GameSerialization
{
    public class GameSerializationException : Exception
    {
        public GameSerializationException(string message) : base(message) { }
        public GameSerializationException() : base() { }
    }
}
