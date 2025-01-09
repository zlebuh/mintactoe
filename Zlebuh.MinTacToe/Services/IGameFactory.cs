using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Services
{
    public interface IGameFactory
    {
        IGameControl Create(Rules rules);
    }
}
