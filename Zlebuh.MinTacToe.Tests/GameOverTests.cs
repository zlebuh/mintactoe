using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Tests
{
    public class GameIsOverCheckTests
    {
        [Fact]
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
            Assert.True(game.GameState.IsGameOver);
            Assert.Equal(Player.O, game.GameState.Winner);
        }
    }
}
