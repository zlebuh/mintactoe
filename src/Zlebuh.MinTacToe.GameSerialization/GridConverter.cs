using System.Text.Json;
using System.Text.Json.Serialization;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization;

internal class GridConverter : JsonConverter<Grid>
{
    public override Grid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Grid grid = [];

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new GameSerializationException();
        }

        Coordinate? coordinate = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (coordinate == null)
            {
                coordinate = JsonSerializer.Deserialize<Coordinate>(ref reader, options);
            }
            else
            {
                Field field = JsonSerializer.Deserialize<Field>(ref reader, options)
                    ?? throw new GameSerializationException("Invalid field");
                grid[coordinate.Value] = field;
                coordinate = null;
            }
        }

        return coordinate != null ? throw new GameSerializationException("Odd number of elements in Grid array") : grid;
    }

    public override void Write(Utf8JsonWriter writer, Grid value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (KeyValuePair<Coordinate, Field> kvp in value)
        {
            JsonSerializer.Serialize(writer, kvp.Key, options);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndArray();
    }
}
