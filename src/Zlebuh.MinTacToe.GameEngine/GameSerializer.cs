using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine
{
    public static class GameSerializer
    {
        private static readonly JsonSerializerOptions options = new();
        static GameSerializer()
        {
            options.Converters.Add(new CoordinateKeyConverter());
            options.Converters.Add(new NullablePlayerEnumConverter());
            options.Converters.Add(new CoordinateConverter());
        }
        public static async Task<string> SerializeGame(Game game)
        {
            using MemoryStream memoryStream = new();
            await JsonSerializer.SerializeAsync(memoryStream, game, options);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(memoryStream);
            string serializedGame = await reader.ReadToEndAsync();
            return serializedGame;
        }

        public static async Task<Game> DeserializeGame(string serializedGame)
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(serializedGame));
            return await JsonSerializer.DeserializeAsync<Game>(stream, options)
                   ?? throw new InvalidOperationException("Deserialization failed.");
        }

        private class CoordinateConverter : JsonConverter<Coordinate>
        {
            public override Coordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                int row = 0;
                int col = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string propertyName = reader.GetString()!;
                    reader.Read();

                    switch (propertyName)
                    {
                        case nameof(Coordinate.Row):
                            row = reader.GetInt32();
                            break;
                        case nameof(Coordinate.Col):
                            col = reader.GetInt32();
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }

                return new Coordinate(row, col);
            }

            public override void Write(Utf8JsonWriter writer, Coordinate value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber(nameof(Coordinate.Row), value.Row);
                writer.WriteNumber(nameof(Coordinate.Col), value.Col);
                writer.WriteEndObject();
            }
        }

        private class NullablePlayerEnumConverter : JsonConverter<Player?>
        {
            public override Player? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }
                string value = reader.GetString() ?? throw new JsonException();
                return Enum.TryParse<Player>(value, out Player player) ? player : throw new JsonException();
            }
            public override void Write(Utf8JsonWriter writer, Player? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteStringValue(value.Value.ToString());
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }

        private class CoordinateKeyConverter : JsonConverter<Grid>
        {
            public override Grid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                Grid grid = new();

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
                    Field field = JsonSerializer.Deserialize<Field>(ref reader, options)
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
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
