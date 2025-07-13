using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization.Tests;

[TestFixture]
public class GameSerializerTests
{
    private GameJsonSerializer serializer;

    [SetUp]
    public void SetUp()
    {
        serializer = new GameJsonSerializer();
    }


    [Test]
    public async Task SerializeAndDeserialize_RoundTrips_GameWithData()
    {
        Game game = new()
        {
            GameState = new GameState
            {
                Grid = new Grid
                {
                    [new Coordinate(0, 0)] = new Field { Player = Player.O, IsMine = false, SurroundedByNotExplodedMines = 1, Generated = true, HasAllNeighboursGenerated = true },
                    [new Coordinate(1, 2)] = new Field { Player = Player.X, IsMine = true, SurroundedByNotExplodedMines = 0, Generated = false, HasAllNeighboursGenerated = false }
                },
                IsGameOver = true,
                Winner = Player.O,
                PlayerOnTurn = Player.X,
                Changes = [new(0, 0), new(1, 2)],
                MovesPlayed = 7
            },
            Rules = new Rules
            {
                Rows = 10,
                Columns = 10,
                SeriesLength = 3,
                NoMineMoves = 2,
                MinePower = 1,
                MineProbability = 0.25
            }
        };

        string json = await serializer.SerializeGameAsync(game);
        Game deserialized = await serializer.DeserializeGameAsync(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized.GameState.IsGameOver, Is.EqualTo(true));
            Assert.That(deserialized.GameState.Winner, Is.EqualTo(Player.O));
            Assert.That(deserialized.GameState.PlayerOnTurn, Is.EqualTo(Player.X));
            Assert.That(deserialized.GameState.MovesPlayed, Is.EqualTo(7));
            Assert.That(deserialized.GameState.Grid, Has.Count.EqualTo(2));
            Assert.That(deserialized.GameState.Grid[new Coordinate(0, 0)].Player, Is.EqualTo(Player.O));
            Assert.That(deserialized.GameState.Grid[new Coordinate(1, 2)].IsMine, Is.True);
            Assert.That(deserialized.GameState.Changes, Is.EquivalentTo(new[] { new Coordinate(0, 0), new Coordinate(1, 2) }));
            Assert.That(deserialized.Rules.Rows, Is.EqualTo(10));
            Assert.That(deserialized.Rules.MineProbability, Is.EqualTo(0.25));
        });
    }

    [Test]
    public async Task Serialize_EmptyGameState_DeserializesCorrectly()
    {
        Game game = new();

        string json = await serializer.SerializeGameAsync(game);
        Game deserialized = await serializer.DeserializeGameAsync(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.GameState, Is.Not.Null);
            Assert.That(deserialized.Rules, Is.Not.Null);
            Assert.That(deserialized.GameState.Grid, Is.Empty);
            Assert.That(deserialized.GameState.Changes, Is.Empty);
        });
    }

    [Test]
    public async Task SerializeAndDeserialize_NullablePlayersHandledCorrectly()
    {
        Game game = new()
        {
            GameState = new GameState
            {
                PlayerOnTurn = null,
                Winner = null
            }
        };

        string json = await serializer.SerializeGameAsync(game);
        Game deserialized = await serializer.DeserializeGameAsync(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized.GameState.PlayerOnTurn, Is.Null);
            Assert.That(deserialized.GameState.Winner, Is.Null);
        });
    }

    [Test]
    public async Task SerializeAndDeserialize_EmptyGridAndChanges()
    {
        Game game = new()
        {
            GameState = new GameState
            {
                Grid = [],
                Changes = []
            }
        };

        string json = await serializer.SerializeGameAsync(game);
        Game deserialized = await serializer.DeserializeGameAsync(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized.GameState.Grid, Is.Empty);
            Assert.That(deserialized.GameState.Changes, Is.Empty);
        });
    }

    [Test]
    public void Deserialize_EmptyString_ThrowsGameSerializationException()
    {
        Assert.ThrowsAsync<GameSerializationException>(() => serializer.DeserializeGameAsync(string.Empty));
    }
}