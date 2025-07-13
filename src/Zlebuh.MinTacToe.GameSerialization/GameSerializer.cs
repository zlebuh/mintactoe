using System.Text;
using System.Text.Json;
using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameSerialization;

public interface ISerializer
{
    Task<string> SerializeGameAsync(Game game);
    Task<Game> DeserializeGameAsync(string serializedGame);
}

public class GameJsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions options = new();
    static GameJsonSerializer()
    {
        options.Converters.Add(new CoordinateConverter());
        options.Converters.Add(new GridConverter());
    }
    public async Task<Game> DeserializeGameAsync(string serializedGame)
    {
        if (string.IsNullOrWhiteSpace(serializedGame))
        {
            throw new GameSerializationException("Serialized game string cannot be null or empty.");
        }
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(serializedGame));
        return await JsonSerializer.DeserializeAsync<Game>(stream, options)
               ?? throw new GameSerializationException();
    }

    public async Task<string> SerializeGameAsync(Game game)
    {
        using MemoryStream memoryStream = new();
        await JsonSerializer.SerializeAsync(memoryStream, game, options);
        memoryStream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(memoryStream);
        string serializedGame = await reader.ReadToEndAsync();
        return serializedGame;
    }
}
