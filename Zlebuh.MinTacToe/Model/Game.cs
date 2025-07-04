﻿namespace Zlebuh.MinTacToe.Model
{
    public class Game
    {
        public GameState GameState { get; internal set; } = new();
        public Rules Rules { get; internal set; } = new();
        public Game MakeCopy()
        {
            return new()
            {
                GameState = new GameState
                {
                    Grid = GameState.Grid.MakeCopy(),
                    IsGameOver = GameState.IsGameOver,
                    Winner = GameState.Winner,
                    PlayerOnTurn = GameState.PlayerOnTurn,
                    Changes = [.. GameState.Changes],
                    MovesPlayed = GameState.MovesPlayed
                },
                Rules = Rules
            };
        }
    }
}
