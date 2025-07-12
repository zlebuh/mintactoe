using NUnit.Framework;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.Tests;

[TestFixture]
public class GameSerializerTests
{
    [Test]
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

        Assert.That(deserializedGame.Rules.Columns, Is.EqualTo(game.Rules.Columns));
        Assert.That(deserializedGame.Rules.Rows, Is.EqualTo(game.Rules.Rows));
        Assert.That(deserializedGame.Rules.SeriesLength, Is.EqualTo(game.Rules.SeriesLength));
        Assert.That(deserializedGame.GameState.IsGameOver, Is.EqualTo(game.GameState.IsGameOver));
        Assert.That(deserializedGame.GameState.Winner, Is.EqualTo(game.GameState.Winner));
        Assert.That(deserializedGame.GameState.PlayerOnTurn, Is.EqualTo(game.GameState.PlayerOnTurn));
        Assert.That(deserializedGame.GameState.MovesPlayed, Is.EqualTo(game.GameState.MovesPlayed));
        Assert.That(deserializedGame.GameState.Changes.Count, Is.EqualTo(game.GameState.Changes.Count));

        for (int i = 0; i < game.GameState.Changes.Count; i++)
        {
            Assert.That(deserializedGame.GameState.Changes[i].Col, Is.EqualTo(game.GameState.Changes[i].Col));
            Assert.That(deserializedGame.GameState.Changes[i].Row, Is.EqualTo(game.GameState.Changes[i].Row));
        }
        for (int i = 0; i < game.Rules.Rows; i++)
        {
            for (int j = 0; j < game.Rules.Columns; j++)
            {
                Coordinate c = new(i, j);
                Assert.That(deserializedGame.GameState.Grid[c].Player, Is.EqualTo(game.GameState.Grid[c].Player));
                Assert.That(deserializedGame.GameState.Grid[c].Generated, Is.EqualTo(game.GameState.Grid[c].Generated));
                Assert.That(deserializedGame.GameState.Grid[c].IsMine, Is.EqualTo(game.GameState.Grid[c].IsMine));
                Assert.That(deserializedGame.GameState.Grid[c].SurroundedByNotExplodedMines, Is.EqualTo(game.GameState.Grid[c].SurroundedByNotExplodedMines));
                Assert.That(deserializedGame.GameState.Grid[c].HasAllNeighboursGenerated, Is.EqualTo(game.GameState.Grid[c].HasAllNeighboursGenerated));
            }
        }
    }
}