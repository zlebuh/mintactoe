using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.ConsoleApp.Services
{
    public class GridView(Rules rules, Game game)
    {
        private readonly Game game = game;
        private readonly int colMin = 0;
        private readonly int colMax = rules.Columns - 1;
        private readonly int rowMin = 0;
        private readonly int rowMax = rules.Rows - 1;

        public void Redraw(bool showMines = false)
        {
            Console.Clear();
            Console.Write("     ||");
            for (int j = colMin; j <= colMax; j++)
            {
                Console.Write($"   {j}".PadRight(6));
                Console.Write("|");
            }
            Console.WriteLine();
            Console.WriteLine("".PadLeft((colMax + 2) * 7, '_'));
            Console.WriteLine("".PadLeft((colMax + 2) * 7, '_'));


            for (int i = rowMin; i <= rowMax; i++)
            {
                Console.WriteLine();
                char c = (char)('a' + i);
                Console.Write($"  {c}  ||");
                for (int j = colMin; j <= colMax; j++)
                {
                    Console.Write("  ");
                    Coordinate coordinate = new(i, j);
                    Field f = game.GameState.Grid[coordinate];
                    if (f.Generated)
                    {
                        if (f.Player.HasValue)
                        {
                            if (f.IsMine)
                            {
                                Console.Write("E ");
                            }
                            else
                            {
                                ConsoleColor foregroundColor = Console.ForegroundColor;
                                Console.ForegroundColor = f.Player == Player.X ? ConsoleColor.Blue : ConsoleColor.Red;
                                Console.Write(f.Player);
                                Console.ForegroundColor = foregroundColor;
                                Console.Write(f.SurroundedByNotExplodedMines);
                            }
                            Console.Write("  |");
                        }
                        else
                        {
                            if (f.IsMine && showMines)
                            {
                                Console.Write("E   |");
                            }
                            else
                            {
                                Console.Write("    |");
                            }
                        }
                    }
                    else
                    {
                        Console.Write("    |");
                    }
                }
                Console.WriteLine();
                Console.WriteLine("".PadLeft((colMax + 2) * 7, '_'));
            }
        }
    }
}