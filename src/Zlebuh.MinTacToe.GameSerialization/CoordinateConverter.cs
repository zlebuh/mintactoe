using System.Text.Json;
using System.Text.Json.Serialization;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization;

internal class CoordinateConverter : JsonConverter<Coordinate>
{
    public override Coordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string s = reader.GetString() ?? throw new JsonException();
        return Read(s);
    }

    public override void Write(Utf8JsonWriter writer, Coordinate value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Write(value));
    }

    private static Coordinate Read(string s)
    {
        string[] parts = s.Split(',');
        return parts.Length != 2
            ? throw new GameSerializationException("Invalid coordinate string format.")
            : !int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int col)
            ? throw new GameSerializationException("Invalid coordinate number.")
            : new Coordinate(row, col);
    }

    private static string Write(Coordinate coordinate)
    {
        return $"{coordinate.Row},{coordinate.Col}";
    }
}
