using NUnit.Framework;
using Zlebuh.MinTacToe.GameEngine.ModelExtensions;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.Tests;

[TestFixture]
public class GameIsOverCheckTests
{
    [Test]
    public void LastMissingPlacedInside()
    {
        Rules rules = new()
        {
            MineProbability = 0,
            Columns = 10,
            Rows = 10,
            SeriesLength = 5
        };
        Game game = GameControl.Initialize(rules);

        game.MakeMove(Player.O, new(0, 0));
        game.MakeMove(Player.X, new(1, 0));
        game.MakeMove(Player.O, new(0, 1));
        game.MakeMove(Player.X, new(2, 0));
        game.MakeMove(Player.O, new(0, 2));
        game.MakeMove(Player.X, new(3, 0));
        game.MakeMove(Player.O, new(0, 4));
        game.MakeMove(Player.X, new(4, 0));
        game.MakeMove(Player.O, new(0, 3));
        Assert.That(game.GameState.IsGameOver, Is.True);
        Assert.That(game.GameState.Winner, Is.EqualTo(Player.O));
    }
}
