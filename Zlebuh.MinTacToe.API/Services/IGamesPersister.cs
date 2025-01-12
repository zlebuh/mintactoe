using Zlebuh.MinTacToe.Services;

namespace Zlebuh.MinTacToe.API.Services
{
    public interface IGamesPersister
    {
        string AddGame(IGameControl gameControl);
        IGameControl? GetGame(string gameId);
        void RemoveGame(string gameId);
    }

    public class GamesPersister : Dictionary<string, IGameControl>, IGamesPersister
    {
        public string AddGame(IGameControl gameControl)
        {
            DateTime dateTime = DateTime.Now;
            string gameId = dateTime.ToString("yyyy-MM-dd-hh-mm-ss");
            Add(gameId, gameControl);
            return gameId;
        }

        public IGameControl? GetGame(string gameId)
        {
            if (TryGetValue(gameId, out IGameControl? gameControl))
            {
                return gameControl;
            }
            return null;
        }

        public void RemoveGame(string gameId)
        {
            Remove(gameId);
        }
    }
}
