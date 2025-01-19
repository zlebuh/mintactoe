using Zlebuh.MinTacToe.Exceptions;

namespace Zlebuh.MinTacToe.Tests
{
    public class GameControlTests
    {
        [Fact]
        public void GameInitialization()
        {
            Game game = GameControl.Initialize(new());
            Assert.Equal(0, game.MovesPlayed);
            Assert.Equal(Player.O, game.GameState.PlayerOnTurn);
            for (int i = 0; i < game.Rules.Rows; i++)
            {
                for (int j = 0; j < game.Rules.Columns; j++)
                {
                    Field f = game.GameState.Grid[new(i, j)];
                    Assert.Null(f.Player);
                }
            }
        }

        [Fact]
        public void CoordinateTests()
        {
            Game game = GameControl.Initialize(new());
            Assert.True(GameControl.IsOnGrid(new(10, 10), game));
            Assert.True(GameControl.IsOnGrid(new(0, 0), game));
            Assert.True(GameControl.IsOnGrid(new(5, 15), game));
            Assert.False(GameControl.IsOnGrid(new(game.Rules.Rows, game.Rules.Columns), game));
            Assert.False(GameControl.IsOnGrid(new(-1, -1), game));
        }

        [Fact]
        public void PlacingAMove()
        {
            Game game = GameControl.Initialize(new()
            {
                MineProbability = 0
            });
            Coordinate coordinate = new(0, 0);
            GameControl.MakeMove(game, Player.O, coordinate);
            Assert.Equal(Player.X, game.GameState.PlayerOnTurn);
            Assert.Equal(coordinate, Assert.Single(game.GameState.Changes));
            Field f = game.GameState.Grid[coordinate];
            Assert.True(f.Player.HasValue);
            Assert.Equal(Player.O, f.Player.Value);
            coordinate = new(1, 0);
            GameControl.MakeMove(game, Player.X, coordinate);
            Assert.Equal(Player.O, game.GameState.PlayerOnTurn);
            Assert.Equal(coordinate, Assert.Single(game.GameState.Changes));
            f = game.GameState.Grid[coordinate];
            Assert.True(f.Player.HasValue);
            Assert.Equal(Player.X, f.Player.Value);
        }

        [Fact]
        public void PlacingAMove_ThrowsOutOfGridException()
        {
            Game game = GameControl.Initialize(new());
            Coordinate coordinate = new(40, 0);
            Assert.Throws<CoordinateOutOfGridException>(() =>
            {
                GameControl.MakeMove(game, Player.O, coordinate);
            });
        }

        [Fact]
        public void PlacingAMove_ThrowsNotOnTurnException()
        {
            Game game = GameControl.Initialize(new());
            Coordinate coordinate = new(0, 0);
            Assert.Throws<NotYourTurnException>(() =>
            {
                GameControl.MakeMove(game, Player.X, coordinate);
            });
        }

        [Fact]
        public void PlacingAMove_ThrowsOccupiedException()
        {
            Game game = GameControl.Initialize(new());
            Coordinate coordinate = new(0, 0);
            GameControl.MakeMove(game, Player.O, coordinate);
            Assert.Throws<FieldOccupiedException>(() =>
            {
                GameControl.MakeMove(game, Player.X, coordinate);
            });
        }

        [Fact]
        public void GamePlayed_OWins()
        {
            Game game = GameControl.Initialize(new()
            {
                Rows = 3,
                Columns = 3,
                SeriesLength = 3,
                MineProbability = 0,
            });

            GameControl.MakeMove(game, Player.O, new(0, 0));
            GameControl.MakeMove(game, Player.X, new(1, 0));
            GameControl.MakeMove(game, Player.O, new(0, 1));
            GameControl.MakeMove(game, Player.X, new(1, 1));
            GameControl.MakeMove(game, Player.O, new(0, 2));
            Assert.True(game.GameState.IsGameOver);
            Assert.True(game.GameState.Winner.HasValue);
            Assert.Equal(Player.O, game.GameState.Winner);
            Assert.False(game.GameState.PlayerOnTurn.HasValue);
        }
        [Fact]
        public void GamePlayed_XWinsDiagonally()
        {
            Game game = GameControl.Initialize(new()
            {
                MineProbability = 0,
            });

            GameControl.MakeMove(game, Player.O, new(0, 0));
            GameControl.MakeMove(game, Player.X, new(10, 10));
            GameControl.MakeMove(game, Player.O, new(0, 1));
            GameControl.MakeMove(game, Player.X, new(11, 11));
            GameControl.MakeMove(game, Player.O, new(0, 2));
            GameControl.MakeMove(game, Player.X, new(12, 12));
            GameControl.MakeMove(game, Player.O, new(0, 3));
            GameControl.MakeMove(game, Player.X, new(13, 13));
            GameControl.MakeMove(game, Player.O, new(0, 10));
            GameControl.MakeMove(game, Player.X, new(14, 14));

            Assert.True(game.GameState.IsGameOver);
            Assert.True(game.GameState.Winner.HasValue);
            Assert.Equal(Player.X, game.GameState.Winner);
            Assert.False(game.GameState.PlayerOnTurn.HasValue);
        }

        [Fact]
        public void SurroundingMinesChanges()
        {
            Game game = GameControl.Initialize(new()
            {
                MineProbability = 1,
                NoMineMoves = 2
            });

            Coordinate coorO = new(10, 10);
            GameControl.MakeMove(game, Player.O, coorO);
            Field fO = game.GameState.Grid[coorO];
            Assert.False(fO.IsMine);
            Assert.Equal(8, fO.SurroundedByNotExplodedMines);
            
            //  90123
            //9 BBB..
            //0 BOB..
            //1 BBB..
            //2 .....
            //3 .....

            Coordinate coorX = new(12, 12);
            GameControl.MakeMove(game, Player.X, coorX);
            Field fX = game.GameState.Grid[coorX];
            Assert.False(fX.IsMine);
            Assert.Equal(8, fX.SurroundedByNotExplodedMines);
            
            //  90123
            //9 BBB..
            //0 BOB..
            //1 BBBBB
            //2 ..BXB
            //3 ..BBB

            Coordinate coorO2 = new(11, 11);
            GameControl.MakeMove(game, Player.O, coorO2);
            
            //  90123
            //9 BBB..
            //0 B.BB.
            //1 BBEBB
            //2 .BBXB
            //3 ..BBB

            Field f02 = game.GameState.Grid[coorO2];
            Assert.True(f02.IsMine);
            Assert.True(f02.Player.HasValue);
            Assert.Equal(Player.O, f02.Player);
            Assert.Equal(6, f02.SurroundedByNotExplodedMines);
            Assert.Equal(7, fO.SurroundedByNotExplodedMines);
            Assert.Equal(7, fX.SurroundedByNotExplodedMines);
        }

        [Fact]
        public void PlacingAMove_ThrowsGameIsOverException()
        {
            Game game = GameControl.Initialize(new()
            {
                SeriesLength = 2,
                MineProbability = 0
            });

            GameControl.MakeMove(game, Player.O, new(0, 0));
            GameControl.MakeMove(game, Player.X, new(0, 1));
            GameControl.MakeMove(game, Player.O, new(1, 0));
            Assert.Throws<GameIsOverException>(() =>
            {
                GameControl.MakeMove(game, Player.X, new(1, 1));
            });
        }
        [Fact]
        public void GamePlayed_Tie()
        {
            Game game = GameControl.Initialize(new()
            {
                Rows = 3,
                Columns = 3,
                SeriesLength = 3,
                MineProbability = 0
            });

            GameControl.MakeMove(game, Player.O, new(0, 0));
            GameControl.MakeMove(game, Player.X, new(1, 1));
            GameControl.MakeMove(game, Player.O, new(0, 1));
            GameControl.MakeMove(game, Player.X, new(0, 2));
            GameControl.MakeMove(game, Player.O, new(2, 0));
            GameControl.MakeMove(game, Player.X, new(1, 0));
            GameControl.MakeMove(game, Player.O, new(1, 2));
            GameControl.MakeMove(game, Player.X, new(2, 1));
            GameControl.MakeMove(game, Player.O, new(2, 2));

            // OOX
            // XXO
            // OXO

            Assert.True(game.GameState.IsGameOver);
            Assert.False(game.GameState.Winner.HasValue);
            Assert.False(game.GameState.PlayerOnTurn.HasValue);
        }

        [Fact]
        public void GamePlayed_WithBomb_Tie()
        {
            Game game = GameControl.Initialize(new()
            {
                Rows = 3,
                Columns = 3,
                SeriesLength = 3,
                MineProbability = 0,
            });

            GameControl.MakeMove(game, Player.O, new(2, 1));
            GameControl.MakeMove(game, Player.X, new(1, 1));
            game.GameState.Grid[new(0, 0)].IsMine = true; // mines indicating counts are corrupted here
            GameControl.MakeMove(game, Player.O, new(2, 0));
            GameControl.MakeMove(game, Player.X, new(2, 2));
            GameControl.MakeMove(game, Player.O, new(1, 0));
            GameControl.MakeMove(game, Player.X, new(1, 2));
            GameControl.MakeMove(game, Player.O, new(0, 2));
            GameControl.MakeMove(game, Player.X, new(0, 1));

            // BXO
            // OXX
            // OOX

            Assert.True(game.GameState.IsGameOver);
            Assert.False(game.GameState.Winner.HasValue);
            Assert.False(game.GameState.PlayerOnTurn.HasValue);
        }

        [Fact]
        public void GamePlayed_WithExplodedBomb_Tie()
        {
            Game game = GameControl.Initialize(new()
            {
                Rows = 3,
                Columns = 3,
                SeriesLength = 3,
                MineProbability = 0,
            });

            GameControl.MakeMove(game, Player.O, new(1, 0));
            GameControl.MakeMove(game, Player.X, new(0, 1));
            game.GameState.Grid[new(0, 0)].IsMine = true; // mines indicating counts are corrupted here
            GameControl.MakeMove(game, Player.O, new(0, 0));
            // bomb erases O from [1, 0]
            Field f = game.GameState.Grid[new(0, 0)];
            Assert.True(f.IsMine);
            Assert.True(f.Player.HasValue);
            Assert.Equal(Player.O, f.Player.Value);
            Field erased = game.GameState.Grid[new(1, 0)];
            Assert.False(erased.IsMine);
            Assert.False(erased.Player.HasValue);

            GameControl.MakeMove(game, Player.X, new(1, 1));
            GameControl.MakeMove(game, Player.O, new(2, 1));
            GameControl.MakeMove(game, Player.X, new(1, 0)); // overlay
            Assert.True(erased.Player.HasValue);
            Assert.Equal(Player.X, erased.Player.Value);
            GameControl.MakeMove(game, Player.O, new(1, 2));
            GameControl.MakeMove(game, Player.X, new(2, 0));
            GameControl.MakeMove(game, Player.O, new(0, 2));
            GameControl.MakeMove(game, Player.X, new(2, 2));

            // BXO
            // XXO
            // XOX

            Assert.True(game.GameState.IsGameOver);
            Assert.False(game.GameState.Winner.HasValue);
            Assert.False(game.GameState.PlayerOnTurn.HasValue);
        }

        [Fact]
        public void Mines_NoMineMovesTest()
        {
            Game game = GameControl.Initialize(new()
            {
                MineProbability = 1,
                NoMineMoves = 2
            });

            GameControl.MakeMove(game, Player.O, new(10, 10));
            Assert.False(game.GameState.Grid[new(10, 10)].IsMine);
            GameControl.MakeMove(game, Player.X, new(5, 5));
            Assert.False(game.GameState.Grid[new(5, 5)].IsMine);
            GameControl.MakeMove(game, Player.O, new(11, 10));
            Assert.True(game.GameState.Grid[new(11, 10)].IsMine);
            GameControl.MakeMove(game, Player.X, new(0, 0));
            Assert.True(game.GameState.Grid[new(0, 0)].IsMine);
        }

        [Fact]
        public void BombExplodedWithNoSurroundings()
        {
            Game game = GameControl.Initialize(new()
            {
                MineProbability = 1,
                NoMineMoves = 2
            });

            GameControl.MakeMove(game, Player.O, new(10, 10));
            GameControl.MakeMove(game, Player.X, new(13, 13));
            GameControl.MakeMove(game, Player.O, new(12, 12));

            Field f1010 = game.GameState.Grid[new(10, 10)];
            Field f1313 = game.GameState.Grid[new(13, 13)];
            Field f1212 = game.GameState.Grid[new(12, 12)];

            Assert.True(f1010.Player.HasValue);
            Assert.Equal(Player.O, f1010.Player.Value);
            Assert.True(f1212.IsMine);
            Assert.True(f1212.Player.HasValue);
            Assert.Equal(Player.O, f1212.Player.Value);
            Assert.True(f1313.Player.HasValue);
            Assert.Equal(Player.X, f1313.Player.Value);
        }

        [Fact]
        public void BombExplodedAndErases()
        {
            Game game = GameControl.Initialize(new()
            {
                MineProbability = 1,
                NoMineMoves = 2
            });

            GameControl.MakeMove(game, Player.O, new(10, 10));
            GameControl.MakeMove(game, Player.X, new(12, 12));
            GameControl.MakeMove(game, Player.O, new(11, 11));

            Field f1010 = game.GameState.Grid[new(10, 10)];
            Field f1111 = game.GameState.Grid[new(11, 11)];
            Field f1212 = game.GameState.Grid[new(12, 12)];

            Assert.False(f1010.Player.HasValue);
            Assert.True(f1111.IsMine);
            Assert.True(f1111.Player.HasValue);
            Assert.Equal(Player.O, f1111.Player.Value);
            Assert.True(f1212.Player.HasValue);
            Assert.Equal(Player.X, f1212.Player.Value);
        }
    }
}