using System.Text.Json;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization.Tests;

[TestFixture]
public class CoordinateConverterTests
{
    private JsonSerializerOptions options;

    [SetUp]
    public void SetUp()
    {
        options = new JsonSerializerOptions();
        options.Converters.Add(new CoordinateConverter());
    }

    [Test]
    public void Serialize_WritesExpectedJsonString()
    {
        Coordinate coordinate = new(3, 7);

        string json = JsonSerializer.Serialize(coordinate, options);

        Assert.That(json, Is.EqualTo("\"3,7\""));
    }

    [Test]
    public void Deserialize_ReadsExpectedCoordinateFromString()
    {
        string json = "\"5,12\"";

        Coordinate coordinate = JsonSerializer.Deserialize<Coordinate>(json, options);

        Assert.Multiple(() =>
        {
            Assert.That(coordinate.Row, Is.EqualTo(5));
            Assert.That(coordinate.Col, Is.EqualTo(12));
        });
    }

    [Test]
    public void Deserialize_InvalidFormat_ThrowsJsonException()
    {
        string json = "\"5;12\""; // wrong separator

        Assert.Throws<GameSerializationException>(() => JsonSerializer.Deserialize<Coordinate>(json, options));
    }

    [Test]
    public void Deserialize_InvalidNumbers_ThrowsJsonException()
    {
        string json = "\"a,b\"";

        Assert.Throws<GameSerializationException>(() => JsonSerializer.Deserialize<Coordinate>(json, options));
    }

    [Test]
    public void Serialize_ThenDeserialize_ReturnsSameCoordinate()
    {
        Coordinate original = new(11, 22);

        string json = JsonSerializer.Serialize(original, options);
        Coordinate result = JsonSerializer.Deserialize<Coordinate>(json, options);

        Assert.Multiple(() =>
        {
            Assert.That(result.Row, Is.EqualTo(original.Row));
            Assert.That(result.Col, Is.EqualTo(original.Col));
        });
    }
}