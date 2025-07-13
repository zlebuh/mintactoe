using System.Text.Json;
using System.Text.Json.Serialization;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization
{
    internal class GridConverter : JsonConverter<Grid>
    {
        public override Grid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Grid grid = [];

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string keyString = reader.GetString() ?? throw new JsonException();
                string[] parts = keyString.Split(",");
                Coordinate coordinate = new(int.Parse(parts[0]), int.Parse(parts[1]));

                reader.Read();
                Field field = System.Text.Json.JsonSerializer.Deserialize<Field>(ref reader, options)
                    ?? throw new JsonException();
                grid[coordinate] = field;
            }

            return grid;
        }

        public override void Write(Utf8JsonWriter writer, Grid value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<Coordinate, Field> kvp in value)
            {
                string key = $"{kvp.Key.Row},{kvp.Key.Col}";
                writer.WritePropertyName(key);
                System.Text.Json.JsonSerializer.Serialize(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
