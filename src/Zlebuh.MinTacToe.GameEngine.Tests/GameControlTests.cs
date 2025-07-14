using NUnit.Framework;
using Zlebuh.MinTacToe.GameEngine.Exceptions;
using Zlebuh.MinTacToe.GameEngine.ModelExtensions;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.Tests;

[TestFixture]

public class GameControlTests
{
    [Test]
    public void GameInitialization()
    {
        Game game = GameControl.Initialize(new());
        Assert.That(game.GameState.MovesPlayed, Is.EqualTo(0));
        Assert.That(game.GameState.PlayerOnTurn, Is.EqualTo(Player.O));
        for (int i = 0; i < game.Rules.Rows; i++)
        {
            for (int j = 0; j < game.Rules.Columns; j++)
            {
                Field f = game.GameState.Grid[new(i, j)];
                Assert.That(f.Player, Is.Null);
            }
        }
    }

    [Test]
    public void CoordinateTests()
    {
        Game game = GameControl.Initialize(new());
        Assert.That(new Coordinate(10, 10).IsOnGrid(game), Is.True);
        Assert.That(new Coordinate(0, 0).IsOnGrid(game), Is.True);
        Assert.That(new Coordinate(5, 15).IsOnGrid(game), Is.True);
        Assert.That(new Coordinate(game.Rules.Rows, game.Rules.Columns).IsOnGrid(game), Is.False);
        Assert.That(new Coordinate(-1, -1).IsOnGrid(game), Is.False);
    }

    [Test]
    public void PlacingAMove()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 0
        });
        Coordinate coordinate = new(0, 0);
        game.MakeMove(Player.O, coordinate);
        Assert.That(game.GameState.PlayerOnTurn, Is.EqualTo(Player.X));
        Assert.That(game.GameState.Changes.Single(), Is.EqualTo(coordinate));
        Field f = game.GameState.Grid[coordinate];
        Assert.That(f.Player.HasValue, Is.True);
        Assert.That(f.Player!.Value, Is.EqualTo(Player.O));
        coordinate = new(1, 0);
        game.MakeMove(Player.X, coordinate);
        Assert.That(game.GameState.PlayerOnTurn, Is.EqualTo(Player.O));
        Assert.That(game.GameState.Changes.Single(), Is.EqualTo(coordinate));
        f = game.GameState.Grid[coordinate];
        Assert.That(f.Player.HasValue, Is.True);
        Assert.That(f.Player!.Value, Is.EqualTo(Player.X));
    }

    [Test]
    public void PlacingAMoveThatCauseMineExplosion_PutsGameToCorrectState()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 0,
        });

        game.MakeMove(Player.O, new(0, 0));
        game.MakeMove(Player.X, new(0, 1));
        game.MakeMove(Player.O, new(0, 2));
        game.MakeMove(Player.X, new(1, 2));
        game.MakeMove(Player.O, new(1, 0));
        game.MakeMove(Player.X, new(2, 0));
        game.MakeMove(Player.O, new(2, 1));
        game.MakeMove(Player.X, new(2, 2));

        // mock
        game.GameState.Grid[new(1, 1)].IsMine = true; // place a mine in the middle
        for (int i = 0; i <= 2; i++)
        {
            for (int j = 0; j <= 2; j++)
            {
                if (i == 1 && j == 1)
                {
                    continue; // skip the mine
                }
                // all fields are surrounded by one not exploded mines
                game.GameState.Grid[new(i, j)].SurroundedByNotExplodedMines += 1;
            }
        }
        // end of mock

        // OXO
        // OMX
        // XOX
        game.MakeMove(Player.O, new(1, 1));

        // -X-
        // -MX
        // X-X

        Assert.That(game.GameState.Changes.Count, Is.EqualTo(9));
        // 8 fields changed because it is not surrounded by mine anymore

        Assert.Multiple(() =>
        {
            Assert.That(game.GameState.Grid[new(1, 1)].IsMine, Is.True);
            Assert.That(game.GameState.Grid[new(1, 1)].Player.HasValue, Is.True);
            Assert.That(game.GameState.Grid[new(1, 1)].Player, Is.EqualTo(Player.O));

            Assert.That(game.GameState.Grid[new(0, 0)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(0, 0)].Player, Is.Null);
            Assert.That(game.GameState.Grid[new(0, 1)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(0, 1)].Player, Is.EqualTo(Player.X));
            Assert.That(game.GameState.Grid[new(0, 2)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(0, 2)].Player, Is.Null);
            Assert.That(game.GameState.Grid[new(1, 0)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(1, 0)].Player, Is.Null);
            Assert.That(game.GameState.Grid[new(1, 2)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(1, 2)].Player, Is.EqualTo(Player.X));
            Assert.That(game.GameState.Grid[new(2, 0)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(2, 0)].Player, Is.EqualTo(Player.X));
            Assert.That(game.GameState.Grid[new(2, 1)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(2, 1)].Player, Is.Null);
            Assert.That(game.GameState.Grid[new(2, 2)].SurroundedByNotExplodedMines, Is.EqualTo(0));
            Assert.That(game.GameState.Grid[new(2, 2)].Player, Is.EqualTo(Player.X));
        });

    }

    [Test]
    public void PlacingAMove_ThrowsOutOfGridException()
    {
        Game game = GameControl.Initialize(new());
        Coordinate coordinate = new(40, 0);
        Assert.Throws<CoordinateOutOfGridException>(() =>
        {
            game.MakeMove(Player.O, coordinate);
        });
    }

    [Test]
    public void PlacingAMove_ThrowsNotOnTurnException()
    {
        Game game = GameControl.Initialize(new());
        Coordinate coordinate = new(0, 0);
        Assert.Throws<NotYourTurnException>(() =>
        {
            game.MakeMove(Player.X, coordinate);
        });
    }

    [Test]
    public void PlacingAMove_ThrowsOccupiedException()
    {
        Game game = GameControl.Initialize(new());
        Coordinate coordinate = new(0, 0);
        game.MakeMove(Player.O, coordinate);
        Assert.Throws<FieldOccupiedException>(() =>
        {
            game.MakeMove(Player.X, coordinate);
        });
    }

    [Test]
    public void GamePlayed_OWins()
    {
        Game game = GameControl.Initialize(new()
        {
            Rows = 3,
            Columns = 3,
            SeriesLength = 3,
            MineProbability = 0,
        });

        game.MakeMove(Player.O, new(0, 0));
        game.MakeMove(Player.X, new(1, 0));
        game.MakeMove(Player.O, new(0, 1));
        game.MakeMove(Player.X, new(1, 1));
        game.MakeMove(Player.O, new(0, 2));
        Assert.That(game.GameState.IsGameOver, Is.True);
        Assert.That(game.GameState.Winner.HasValue, Is.True);
        Assert.That(game.GameState.Winner, Is.EqualTo(Player.O));
        Assert.That(game.GameState.PlayerOnTurn.HasValue, Is.False);
    }

    [Test]
    public void GamePlayed_XWinsDiagonally()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 0,
        });

        game.MakeMove(Player.O, new(0, 0));
        game.MakeMove(Player.X, new(10, 10));
        game.MakeMove(Player.O, new(0, 1));
        game.MakeMove(Player.X, new(11, 11));
        game.MakeMove(Player.O, new(0, 2));
        game.MakeMove(Player.X, new(12, 12));
        game.MakeMove(Player.O, new(0, 3));
        game.MakeMove(Player.X, new(13, 13));
        game.MakeMove(Player.O, new(0, 10));
        game.MakeMove(Player.X, new(14, 14));

        Assert.That(game.GameState.IsGameOver, Is.True);
        Assert.That(game.GameState.Winner.HasValue, Is.True);
        Assert.That(game.GameState.Winner, Is.EqualTo(Player.X));
        Assert.That(game.GameState.PlayerOnTurn.HasValue, Is.False);
    }

    [Test]
    public void SurroundingMinesChanges()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 1,
            NoMineMoves = 2
        });

        Coordinate coorO = new(10, 10);
        game.MakeMove(Player.O, coorO);
        Field fO = game.GameState.Grid[coorO];
        Assert.That(fO.IsMine, Is.False);
        Assert.That(fO.SurroundedByNotExplodedMines, Is.EqualTo(8));

        //  90123
        //9 BBB..
        //0 BOB..
        //1 BBB..
        //2 .....
        //3 .....

        Coordinate coorX = new(12, 12);
        game.MakeMove(Player.X, coorX);
        Field fX = game.GameState.Grid[coorX];
        Assert.That(fX.IsMine, Is.False);
        Assert.That(fX.SurroundedByNotExplodedMines, Is.EqualTo(8));

        //  90123
        //9 BBB..
        //0 BOB..
        //1 BBBBB
        //2 ..BXB
        //3 ..BBB

        Coordinate coorO2 = new(11, 11);
        game.MakeMove(Player.O, coorO2);

        //  90123
        //9 BBB..
        //0 B.BB.
        //1 BBEBB
        //2 .BBXB
        //3 ..BBB

        Field f02 = game.GameState.Grid[coorO2];
        Assert.That(f02.IsMine, Is.True);
        Assert.That(f02.Player.HasValue, Is.True);
        Assert.That(f02.Player, Is.EqualTo(Player.O));
        Assert.That(f02.SurroundedByNotExplodedMines, Is.EqualTo(6));
        Assert.That(fO.SurroundedByNotExplodedMines, Is.EqualTo(7));
        Assert.That(fX.SurroundedByNotExplodedMines, Is.EqualTo(7));
    }

    [Test]
    public void PlacingAMove_ThrowsGameIsOverException()
    {
        Game game = GameControl.Initialize(new()
        {
            SeriesLength = 2,
            MineProbability = 0
        });

        game.MakeMove(Player.O, new(0, 0));
        game.MakeMove(Player.X, new(0, 1));
        game.MakeMove(Player.O, new(1, 0));
        Assert.Throws<GameIsOverException>(() =>
        {
            game.MakeMove(Player.X, new(1, 1));
        });
    }

    [Test]
    public void GamePlayed_Tie()
    {
        Game game = GameControl.Initialize(new()
        {
            Rows = 3,
            Columns = 3,
            SeriesLength = 3,
            MineProbability = 0
        });

        game.MakeMove(Player.O, new(0, 0));
        game.MakeMove(Player.X, new(1, 1));
        game.MakeMove(Player.O, new(0, 1));
        game.MakeMove(Player.X, new(0, 2));
        game.MakeMove(Player.O, new(2, 0));
        game.MakeMove(Player.X, new(1, 0));
        game.MakeMove(Player.O, new(1, 2));
        game.MakeMove(Player.X, new(2, 1));
        game.MakeMove(Player.O, new(2, 2));

        // OOX
        // XXO
        // OXO

        Assert.That(game.GameState.IsGameOver, Is.True);
        Assert.That(game.GameState.Winner.HasValue, Is.False);
        Assert.That(game.GameState.PlayerOnTurn.HasValue, Is.False);
    }

    [Test]
    public void GamePlayed_WithBomb_Tie()
    {
        Game game = GameControl.Initialize(new()
        {
            Rows = 3,
            Columns = 3,
            SeriesLength = 3,
            MineProbability = 0,
        });

        game.MakeMove(Player.O, new(2, 1));
        game.MakeMove(Player.X, new(1, 1));
        game.GameState.Grid[new(0, 0)].IsMine = true; // mines indicating counts are corrupted here
        game.MakeMove(Player.O, new(2, 0));
        game.MakeMove(Player.X, new(2, 2));
        game.MakeMove(Player.O, new(1, 0));
        game.MakeMove(Player.X, new(1, 2));
        game.MakeMove(Player.O, new(0, 2));
        game.MakeMove(Player.X, new(0, 1));

        // BXO
        // OXX
        // OOX

        Assert.That(game.GameState.IsGameOver, Is.True);
        Assert.That(game.GameState.Winner.HasValue, Is.False);
        Assert.That(game.GameState.PlayerOnTurn.HasValue, Is.False);
    }

    [Test]
    public void GamePlayed_WithExplodedBomb_Tie()
    {
        Game game = GameControl.Initialize(new()
        {
            Rows = 3,
            Columns = 3,
            SeriesLength = 3,
            MineProbability = 0,
        });

        game.MakeMove(Player.O, new(1, 0));
        game.MakeMove(Player.X, new(0, 1));
        game.GameState.Grid[new(0, 0)].IsMine = true; // mines indicating counts are corrupted here
        game.MakeMove(Player.O, new(0, 0));
        // bomb erases O from [1, 0]
        Field f = game.GameState.Grid[new(0, 0)];
        Assert.That(f.IsMine, Is.True);
        Assert.That(f.Player.HasValue, Is.True);
        Assert.That(f.Player!.Value, Is.EqualTo(Player.O));
        Field erased = game.GameState.Grid[new(1, 0)];
        Assert.That(erased.IsMine, Is.False);
        Assert.That(erased.Player.HasValue, Is.False);

        game.MakeMove(Player.X, new(1, 1));
        game.MakeMove(Player.O, new(2, 1));
        game.MakeMove(Player.X, new(1, 0)); // overlay
        Assert.That(erased.Player.HasValue, Is.True);
        Assert.That(erased.Player!.Value, Is.EqualTo(Player.X));
        game.MakeMove(Player.O, new(1, 2));
        game.MakeMove(Player.X, new(2, 0));
        game.MakeMove(Player.O, new(0, 2));
        game.MakeMove(Player.X, new(2, 2));

        // BXO
        // XXO
        // XOX

        Assert.That(game.GameState.IsGameOver, Is.True);
        Assert.That(game.GameState.Winner.HasValue, Is.False);
        Assert.That(game.GameState.PlayerOnTurn.HasValue, Is.False);
    }

    [Test]
    public void Mines_NoMineMovesTest()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 1,
            NoMineMoves = 2
        });

        game.MakeMove(Player.O, new(10, 10));
        Assert.That(game.GameState.Grid[new(10, 10)].IsMine, Is.False);
        game.MakeMove(Player.X, new(5, 5));
        Assert.That(game.GameState.Grid[new(5, 5)].IsMine, Is.False);
        game.MakeMove(Player.O, new(11, 10));
        Assert.That(game.GameState.Grid[new(11, 10)].IsMine, Is.True);
        game.MakeMove(Player.X, new(0, 0));
        Assert.That(game.GameState.Grid[new(0, 0)].IsMine, Is.True);
    }

    [Test]
    public void BombExplodedWithNoSurroundings()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 1,
            NoMineMoves = 2
        });

        game.MakeMove(Player.O, new(10, 10));
        game.MakeMove(Player.X, new(13, 13));
        game.MakeMove(Player.O, new(12, 12));

        Field f1010 = game.GameState.Grid[new(10, 10)];
        Field f1313 = game.GameState.Grid[new(13, 13)];
        Field f1212 = game.GameState.Grid[new(12, 12)];

        Assert.That(f1010.Player.HasValue, Is.True);
        Assert.That(f1010.Player!.Value, Is.EqualTo(Player.O));
        Assert.That(f1212.IsMine, Is.True);
        Assert.That(f1212.Player.HasValue, Is.True);
        Assert.That(f1212.Player!.Value, Is.EqualTo(Player.O));
        Assert.That(f1313.Player.HasValue, Is.True);
        Assert.That(f1313.Player!.Value, Is.EqualTo(Player.X));
    }

    [Test]
    public void BombExplodedAndErases()
    {
        Game game = GameControl.Initialize(new()
        {
            MineProbability = 1,
            NoMineMoves = 2
        });

        Field f1010 = game.GameState.Grid[new(10, 10)];
        Field f1111 = game.GameState.Grid[new(11, 11)];
        Field f1212 = game.GameState.Grid[new(12, 12)];

        game.MakeMove(Player.O, new(10, 10));
        game.MakeMove(Player.X, new(12, 12));
        game.MakeMove(Player.O, new(11, 11));
        Assert.That(f1010.Player.HasValue, Is.False);
        Assert.That(f1111.IsMine, Is.True);
        Assert.That(f1111.Player.HasValue, Is.True);
        Assert.That(f1111.Player!.Value, Is.EqualTo(Player.O));
        Assert.That(f1212.Player.HasValue, Is.True);
        Assert.That(f1212.Player!.Value, Is.EqualTo(Player.X));
        game.MakeMove(Player.X, new(10, 10));
    }
}