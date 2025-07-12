using Zlebuh.MinTacToe.GameEngine;
using Zlebuh.MinTacToe.GameEngine.Model;

namespace Zlebuh.MinTacToe.ConsoleApp.Players
{
    internal class Human : IAIPlayer
    {
        public Coordinate MakeMove(Game game, Player player)
        {
            while (true)
            {
                Console.Write($"{player} plays: ");
                string input = Console.ReadLine()!;
                try
                {
                    char rowChar = char.Parse(input[0..1].ToLower());
                    int col = int.Parse(input[1..]);
                    int row = rowChar - 'a';
                    if (row < 0 || row >= game.Rules.Rows) continue;
                    if (col < 0 || col >= game.Rules.Columns) continue;
                    return new(row, col);
                }
                catch
                {

                }
            }
        }
    }
}
