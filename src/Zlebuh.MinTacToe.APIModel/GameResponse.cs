﻿namespace Zlebuh.MinTacToe.APIModel
{
    public class GameResponse
    {
        public string GameId { get; set; } = string.Empty;
        public string HostToken { get; set; } = string.Empty;
        public string VisitorToken { get; set; } = string.Empty;
    }
}
