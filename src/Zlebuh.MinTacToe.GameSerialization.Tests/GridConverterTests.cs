using System.Text.Json;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization.Tests;

[TestFixture]
public class GridConverterTests
{
    private JsonSerializerOptions options;

    [SetUp]
    public void SetUp()
    {
        options = new JsonSerializerOptions();
        options.Converters.Add(new CoordinateConverter());
        options.Converters.Add(new GridConverter());
    }

    [Test]
    public void Serialize_WritesExpectedJsonArray()
    {
        Grid grid = new()
        {
            [new Coordinate(1, 2)] = new Field { Player = Player.X, SurroundedByNotExplodedMines = 3, IsMine = true, Generated = false, HasAllNeighboursGenerated = true },
            [new Coordinate(2, 3)] = new Field { Player = Player.O, SurroundedByNotExplodedMines = 1, IsMine = false, Generated = true, HasAllNeighboursGenerated = false }
        };

        string json = JsonSerializer.Serialize(grid, options);

        // Should be an array alternating coordinate and field objects
        // Example:
        // [
        //   {"Row":1,"Col":2},{"Player":1,"SurroundedByNotExplodedMines":3,"IsMine":true,"Generated":false,"HasAllNeighboursGenerated":true},
        //   {"Row":2,"Col":3},{"Player":0,"SurroundedByNotExplodedMines":1,"IsMine":false,"Generated":true,"HasAllNeighboursGenerated":false}
        // ]
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(4));
        });

        JsonElement.ArrayEnumerator arr = doc.RootElement.EnumerateArray();
        arr.MoveNext();
        JsonElement coord1 = arr.Current;
        arr.MoveNext();
        JsonElement field1 = arr.Current;
        arr.MoveNext();
        JsonElement coord2 = arr.Current;
        arr.MoveNext();
        JsonElement field2 = arr.Current;

        Assert.Multiple(() =>
        {
            Assert.That(coord1.GetString(), Is.EqualTo("1,2"));
            Assert.That(field1.GetProperty("Player").GetInt32(), Is.EqualTo((int)Player.X));
            Assert.That(field1.GetProperty("IsMine").GetBoolean(), Is.True);

            Assert.That(coord2.GetString(), Is.EqualTo("2,3"));
            Assert.That(field2.GetProperty("Player").GetInt32(), Is.EqualTo((int)Player.O));
            Assert.That(field2.GetProperty("HasAllNeighboursGenerated").GetBoolean(), Is.False);
        });
    }

    [Test]
    public void Deserialize_ReadsExpectedGrid()
    {
        string json = @"
                [
                    ""5,10"",{""Player"":1,""SurroundedByNotExplodedMines"":2,""IsMine"":false,""Generated"":true,""HasAllNeighboursGenerated"":false},
                    ""0,1"",{""Player"":0,""SurroundedByNotExplodedMines"":0,""IsMine"":true,""Generated"":false,""HasAllNeighboursGenerated"":true}
                ]";

        Grid grid = JsonSerializer.Deserialize<Grid>(json, options)!;

        Assert.That(grid, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(grid[new Coordinate(5, 10)].Player, Is.EqualTo(Player.X));
            Assert.That(grid[new Coordinate(5, 10)].SurroundedByNotExplodedMines, Is.EqualTo(2));
            Assert.That(grid[new Coordinate(5, 10)].IsMine, Is.False);

            Assert.That(grid[new Coordinate(0, 1)].Player, Is.EqualTo(Player.O));
            Assert.That(grid[new Coordinate(0, 1)].IsMine, Is.True);
            Assert.That(grid[new Coordinate(0, 1)].HasAllNeighboursGenerated, Is.True);
        });
    }

    [Test]
    public void SerializeThenDeserialize_ReturnsEquivalentGrid()
    {
        Grid grid = new()
        {
            [new Coordinate(7, 8)] = new Field { Player = Player.X, SurroundedByNotExplodedMines = 4, IsMine = true, Generated = false, HasAllNeighboursGenerated = true },
            [new Coordinate(2, 4)] = new Field { Player = Player.O, SurroundedByNotExplodedMines = 0, IsMine = false, Generated = true, HasAllNeighboursGenerated = false }
        };

        string json = JsonSerializer.Serialize(grid, options);
        Grid deserialized = JsonSerializer.Deserialize<Grid>(json, options)!;

        Assert.That(deserialized, Has.Count.EqualTo(grid.Count));
        foreach (KeyValuePair<Coordinate, Field> kvp in grid)
        {
            Assert.That(deserialized, Does.ContainKey(kvp.Key));
            Field field = deserialized[kvp.Key];
            Assert.Multiple(() =>
            {
                Assert.That(field.Player, Is.EqualTo(kvp.Value.Player));
                Assert.That(field.SurroundedByNotExplodedMines, Is.EqualTo(kvp.Value.SurroundedByNotExplodedMines));
                Assert.That(field.IsMine, Is.EqualTo(kvp.Value.IsMine));
                Assert.That(field.Generated, Is.EqualTo(kvp.Value.Generated));
                Assert.That(field.HasAllNeighboursGenerated, Is.EqualTo(kvp.Value.HasAllNeighboursGenerated));
            });
        }
    }

    [Test]
    public void Deserialize_InvalidToken_ThrowsGameSerializationException()
    {
        string json = @"{ ""not"": ""an array"" }";
        Assert.Throws<GameSerializationException>(() => JsonSerializer.Deserialize<Grid>(json, options));
    }

    [Test]
    public void Deserialize_OddNumberOfElements_ThrowsGameSerializationException()
    {
        string json = @"[
                ""1,2"",
                {""Player"":1,""SurroundedByNotExplodedMines"":0,""IsMine"":false,""Generated"":true,""HasAllNeighboursGenerated"":true},
                ""2,3""
            ]";
        Assert.Throws<GameSerializationException>(() => JsonSerializer.Deserialize<Grid>(json, options));
    }
}