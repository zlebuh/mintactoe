using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.API.Model
{
    public class MakeMoveRequest
    {
        public string GameId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public Coordinate Coordinate { get; set; }
    }
}
