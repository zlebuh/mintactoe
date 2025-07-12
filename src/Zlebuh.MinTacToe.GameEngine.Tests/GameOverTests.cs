using NUnit.Framework;
using Zlebuh.MinTacToe.GameEngine.Model;

namespace Zlebuh.MinTacToe.GameEngine.Tests
{
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

            GameControl.MakeMove(game, Player.O, new(0, 0));
            GameControl.MakeMove(game, Player.X, new(1, 0));
            GameControl.MakeMove(game, Player.O, new(0, 1));
            GameControl.MakeMove(game, Player.X, new(2, 0));
            GameControl.MakeMove(game, Player.O, new(0, 2));
            GameControl.MakeMove(game, Player.X, new(3, 0));
            GameControl.MakeMove(game, Player.O, new(0, 4));
            GameControl.MakeMove(game, Player.X, new(4, 0));
            GameControl.MakeMove(game, Player.O, new(0, 3));
            Assert.That(game.GameState.IsGameOver, Is.True);
            Assert.That(game.GameState.Winner, Is.EqualTo(Player.O));
        }
    }
}
