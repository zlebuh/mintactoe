using Zlebuh.MinTacToe.GameEngine;
using Zlebuh.MinTacToe.GameEngine.Model;

namespace Zlebuh.MinTacToe.Tests
{
    public class GameSerializerTests
    {
        [Fact]
        public async Task SerializeAndDeserializeGame()
        {
            Rules rules = new()
            {
                MineProbability = 0,
                Columns = 3,
                Rows = 3,
                SeriesLength = 3
            };
            // xox
            // oxo
            // oxo
            Game game = GameControl.Initialize(rules);
            GameControl.MakeMove(game, Player.O, new(0, 1));
            GameControl.MakeMove(game, Player.X, new(1, 1));
            GameControl.MakeMove(game, Player.O, new(1, 0));
            GameControl.MakeMove(game, Player.X, new(0, 0));
            GameControl.MakeMove(game, Player.O, new(1, 2));
            GameControl.MakeMove(game, Player.X, new(2, 1));
            GameControl.MakeMove(game, Player.O, new(2, 0));
            GameControl.MakeMove(game, Player.X, new(0, 2));
            GameControl.MakeMove(game, Player.O, new(2, 2));
            string serializedGame = await GameSerializer.SerializeGame(game);
            Game deserializedGame = await GameSerializer.DeserializeGame(serializedGame);
            Assert.Equal(game.Rules.Columns, deserializedGame.Rules.Columns);
            Assert.Equal(game.Rules.Rows, deserializedGame.Rules.Rows);
            Assert.Equal(game.Rules.SeriesLength, deserializedGame.Rules.SeriesLength);
            Assert.Equal(game.GameState.IsGameOver, deserializedGame.GameState.IsGameOver);
            Assert.Equal(game.GameState.Winner, deserializedGame.GameState.Winner);
            Assert.Equal(game.GameState.PlayerOnTurn, deserializedGame.GameState.PlayerOnTurn);
            Assert.Equal(game.GameState.MovesPlayed, deserializedGame.GameState.MovesPlayed);
            Assert.Equal(game.GameState.Changes.Count, deserializedGame.GameState.Changes.Count);
            for (int i = 0; i < game.GameState.Changes.Count; i++)
            {
                Assert.Equal(game.GameState.Changes[i].Col, deserializedGame.GameState.Changes[i].Col);
                Assert.Equal(game.GameState.Changes[i].Row, deserializedGame.GameState.Changes[i].Row);
            }
            for (int i = 0; i < game.Rules.Rows; i++)
            {
                for (int j = 0; j < game.Rules.Columns; j++)
                {
                    Coordinate c = new(i, j);
                    Assert.Equal(game.GameState.Grid[c].Player, deserializedGame.GameState.Grid[c].Player);
                    Assert.Equal(game.GameState.Grid[c].Generated, deserializedGame.GameState.Grid[c].Generated);
                    Assert.Equal(game.GameState.Grid[c].IsMine, deserializedGame.GameState.Grid[c].IsMine);
                    Assert.Equal(game.GameState.Grid[c].SurroundedByNotExplodedMines, deserializedGame.GameState.Grid[c].SurroundedByNotExplodedMines);
                    Assert.Equal(game.GameState.Grid[c].HasAllNeighboursGenerated, deserializedGame.GameState.Grid[c].HasAllNeighboursGenerated);
                }
            }
        }
    }
}
