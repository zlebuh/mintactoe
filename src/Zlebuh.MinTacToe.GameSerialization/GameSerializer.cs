using System.Text;
using System.Text.Json;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization
{
    public interface ISerializer
    {
        Task<string> SerializeGame(Game game);
        Task<Game> DeserializeGame(string serializedGame);
    }

    public class JsonSerializer : ISerializer
    {
        private static readonly JsonSerializerOptions options = new();
        static JsonSerializer()
        {
            options.Converters.Add(new GridConverter());
            options.Converters.Add(new CoordinateConverter());
        }
        public async Task<Game> DeserializeGame(string serializedGame)
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(serializedGame));
            return await System.Text.Json.JsonSerializer.DeserializeAsync<Game>(stream, options)
                   ?? throw new InvalidOperationException("Deserialization failed.");
        }

        public async Task<string> SerializeGame(Game game)
        {
            using MemoryStream memoryStream = new();
            await System.Text.Json.JsonSerializer.SerializeAsync(memoryStream, game, options);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(memoryStream);
            string serializedGame = await reader.ReadToEndAsync();
            return serializedGame;
        }
    }
}
