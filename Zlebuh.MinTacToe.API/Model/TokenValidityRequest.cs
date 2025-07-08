namespace Zlebuh.MinTacToe.API.Model
{
    public class TokenValidityRequest
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
    }
}
