using System.Text.Json;
using System.Text.Json.Serialization;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization
{
    internal class CoordinateConverter : JsonConverter<Coordinate>
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
}
