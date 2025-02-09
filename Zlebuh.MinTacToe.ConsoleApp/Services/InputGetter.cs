using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.ConsoleApp.Services
{
    public class InputGetter(IAIPlayer x, IAIPlayer o, Game game)
    {
        private readonly IAIPlayer x = x;
        private readonly IAIPlayer o = o;
        private readonly Game game = game;

        public Coordinate GetInput(Player player)
        {
            return player switch
            {
                Player.X => x.MakeMove(game, player),
                Player.O => o.MakeMove(game, player),
                _ => throw new InvalidOperationException("Invalid player.")
            };
        }
    }
}