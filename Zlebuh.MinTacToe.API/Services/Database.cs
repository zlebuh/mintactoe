namespace Zlebuh.MinTacToe.API.Services
{
    public interface IDatabase
    {
        Task<bool> UserExistsAsync(string userId);
        Task<string> CreateUserAsync();
        Task<(string gameId, string hostToken, string visitorToken)> CreateGame(string hostUserId);
        Task<(string hostToken, string visitorToken, string hostUserId, string? visitorUserId)> GetGameDetails(string gameId);
        Task SetVisitorUserId(string gameId, string userId);
        Task<string> GetLatestGameState(string gameId);
        Task AddGameState(string gameId, string newGameState);
    }

    internal class Database : IDatabase
    {
        public Task AddGameState(string gameId, string newGameState)
        {
            throw new NotImplementedException();
        }

        public Task<(string gameId, string hostToken, string visitorToken)> CreateGame(string hostUserId)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<(string hostToken, string visitorToken, string hostUserId, string? visitorUserId)> GetGameDetails(string gameId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLatestGameState(string gameId)
        {
            throw new NotImplementedException();
        }

        public Task SetVisitorUserId(string gameId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserExistsAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
