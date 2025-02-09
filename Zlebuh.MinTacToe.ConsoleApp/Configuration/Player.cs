namespace Zlebuh.MinTacToe.ConsoleApp.Configuration
{
    public class Player
    {
        public string PlayerType { get; set; }
        public Dictionary<string, object> PlayerProperties { get; set; } = new();
    }
}
