﻿namespace Zlebuh.MinTacToe.APIModel
{
    public class MakeMoveRequest
    {
        public string GameId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int CoordinateRow { get; set; }
        public int CoordinateCol { get; set; }
    }
}
