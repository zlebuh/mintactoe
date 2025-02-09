using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.ConsoleApp.Configuration
{
    public class OneGame
    {
        public int MaxMilisecondsPerMove { get; set; }
        public int MinMilisecondsPerMove { get; set; }
        public Opponents Opponents { get; set; }
        public Rules Rules { get; set; }
    }
}
