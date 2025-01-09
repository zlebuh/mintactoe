using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Services.Implementations
{
    public class GameFactory : IGameFactory
    {
        public IGameControl Create(Rules rules)
        {
            IMineAuthority mineAuthority = new MineAuthority(rules.MineProbability, rules.NoMineMoves);
            Dimension dimension = new(rules.Rows, rules.Columns);
            Grid grid = new(dimension, mineAuthority, rules.MinePower);
            IReferee referee = new Referee(grid, rules.SeriesLength, dimension);
            GameControl control = new(grid, referee);
            return control;
        }
    }
}
