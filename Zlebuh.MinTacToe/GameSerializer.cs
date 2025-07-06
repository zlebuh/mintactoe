using System.Text;
using System.Text.Json;
using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe
{
    public static class GameSerializer
    {
        public static async Task<string> SerializeGame(Game game)
        {
            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, game);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(memoryStream);
            string serializedGame = await reader.ReadToEndAsync();
            return serializedGame;
        }

        public static async Task<Game> DeserializeGame(string serializedGame)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedGame));
            return await JsonSerializer.DeserializeAsync<Game>(stream)
                   ?? throw new InvalidOperationException("Deserialization failed.");
        }
    }
}
