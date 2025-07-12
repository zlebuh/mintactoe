using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Zlebuh.MinTacToe.API.Services
{
    public interface IDatabase
    {
        Task<bool> UserExistsAsync(string userId);
        Task<string> CreateUserAsync();
        Task<(string gameId, string hostToken, string visitorToken)> CreateGame(string hostUserId, string gameState);
        Task<(string hostToken, string visitorToken, string hostUserId, string? visitorUserId)> GetGameDetails(string gameId);
        Task SetVisitorUserId(string gameId, string userId);
        Task<string> GetGameState(string gameId);
        Task UpdateGameState(string gameId, string newGameState);
    }

    public class SupabaseCredentials
    {
        public string Url { get; set; } = string.Empty;
        public string AnonKey { get; set; } = string.Empty;
    }

    [Table("users")]
    public class UserDbModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        [Column("nickname")]
        public string Nickname { get; set; } = string.Empty;
    }

    [Table("games")]
    public class GameDbModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("created_at", ignoreOnInsert: true)]
        public DateTime CreatedAt { get; set; }

        [Column("host_token", ignoreOnInsert: true)]
        public string HostToken { get; set; } = string.Empty;

        [Column("invited_token", ignoreOnInsert: true)]
        public string InvitedToken { get; set; } = string.Empty;

        [Column("host_user_id")]
        public Guid HostUserId { get; set; }

        [Column("invited_user_id", ignoreOnInsert: true)]
        public Guid? InvitedUserId { get; set; }

        [Column("updated_at", ignoreOnInsert: true)]
        public DateTime? UpdatedAt { get; set; }

        [Column("game_state")]
        public string GameState { get; set; } = string.Empty;
    }


    internal class SupabaseDatabase : IDatabase
    {
        private readonly Client supabase;

        public SupabaseDatabase(IOptions<SupabaseCredentials> supabaseCredentials)
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            supabase = new Client(supabaseCredentials.Value.Url, supabaseCredentials.Value.AnonKey, options);            
        }
        public async Task UpdateGameState(string gameId, string newGameState)
        {
            var gameGuid = Guid.Parse(gameId);
            await supabase.From<GameDbModel>()
                .Where(g => g.Id == gameGuid)
                .Set(g => g.GameState, newGameState)
                .Update();                    
        }

        public async Task<(string gameId, string hostToken, string visitorToken)> CreateGame(string hostUserId, string gameState)
        {
            GameDbModel g = new()
            {
                HostUserId = Guid.Parse(hostUserId),
                GameState = gameState
            };
            var res = await supabase.From<GameDbModel>()
                .Insert(g);

            var id = res.Model?.Id ?? throw new Exception("Game creation failed. No ID returned.");

            var createdGame = await supabase.From<GameDbModel>()
                .Where(g => g.Id == id)
                .Single();            

            return createdGame == null
                ? throw new InvalidOperationException("Game creation failed. No game found with the created ID.")
                : (createdGame.Id.ToString(), createdGame.HostToken, createdGame.InvitedToken);
        }

        public async Task<string> CreateUserAsync()
        {
            UserDbModel user = new();
            var res = await supabase.From<UserDbModel>()
                .Insert(user);                

            var createdUser = res.Model ?? throw new InvalidOperationException("User creation failed.");
            return createdUser.Id.ToString();

        }        

        public async Task<(string hostToken, string visitorToken, string hostUserId, string? visitorUserId)> GetGameDetails(string gameId)
        {
            var guid = Guid.Parse(gameId);
            var game = await supabase.From<GameDbModel>()
                .Where(g => g.Id == guid)
                .Single() ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
            
            return (game.HostToken, game.InvitedToken, game.HostUserId.ToString(), game.InvitedUserId?.ToString());
        }

        public async Task<string> GetGameState(string gameId)
        {
            var gameGuid = Guid.Parse(gameId);
            var game = await supabase.From<GameDbModel>()
                .Where(g => g.Id == gameGuid)
                .Single() ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");            
            return game.GameState;
        }

        public Task SetVisitorUserId(string gameId, string userId)
        {
            var visitorUserId = Guid.Parse(userId);
            var gameGuid = Guid.Parse(gameId);
            return supabase.From<GameDbModel>()
                .Where(g => g.Id == gameGuid)
                .Set(g => g.InvitedUserId!, visitorUserId)
                .Update();
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            var guid = Guid.Parse(userId);

            var result = await supabase.From<UserDbModel>()
                .Where(x => x.Id == guid)
                .Single();

            return result != null;
        }
    }
}
