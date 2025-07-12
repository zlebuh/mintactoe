using Zlebuh.MinTacToe.ConsoleApp.Configuration;
using Zlebuh.MinTacToe.GameEngine;
using Zlebuh.MinTacToe.GameEngine.Exceptions;
using Zlebuh.MinTacToe.GameEngine.Model;

namespace Zlebuh.MinTacToe.ConsoleApp.Services
{
    public class GameSimulator
    {
        private readonly InputGetter inputGetter;
        private readonly Rules rules;
        private readonly Game game;
        private readonly GridView gridView;

        public GameSimulator(OneGame gameConfiguration)
        {
            rules = gameConfiguration.Rules;
            game = GameControl.Initialize(rules);
            PlayerActivator playerActivator = new();
            IAIPlayer x = playerActivator.Activate(gameConfiguration.Opponents.X);
            IAIPlayer o = playerActivator.Activate(gameConfiguration.Opponents.O);
            inputGetter = new InputGetter(x, o, game);
            gridView = new GridView(rules, game);

        }
        public void Play()
        {
            gridView.Redraw();

            if (!game.GameState.PlayerOnTurn.HasValue)
            {
                throw new InvalidOperationException("Player on turn is not set.");
            }

            do
            {
                Coordinate input = inputGetter.GetInput(game.GameState.PlayerOnTurn.Value);
                try
                {
                    GameControl.MakeMove(game, game.GameState.PlayerOnTurn.Value, input);
                }
                catch (MinTacToeException ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
                gridView.Redraw();
            } while (!game.GameState.IsGameOver);
            gridView.Redraw(showMines: true);

            Console.WriteLine($"The result is: {(game.GameState.Winner.HasValue ? game.GameState.Winner : "No one")}.");
        }
    }
}