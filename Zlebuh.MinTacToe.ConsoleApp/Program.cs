using Zlebuh.MinTacToe.Exceptions;
using Zlebuh.MinTacToe.Model;
using Zlebuh.MinTacToe.Services;
using Zlebuh.MinTacToe.Services.Implementations;

IGameFactory gameFactory = new GameFactory();
Rules rules = new();
int colMin = 0;
int colMax = rules.Columns - 1;
int rowMin = 0;
int rowMax = rules.Rows - 1;
IGameControl gameControl = gameFactory.Create(rules);

RedrawField(gameControl.Grid);

MoveOutcome outcome;
do
{
    Coordinate input = GetInput(gameControl.PlayerOnMove);
    try
    {
        outcome = gameControl.PlaceMove(gameControl.PlayerOnMove, input);
    }
    catch (FieldOccupiedException ex)
    {
        Console.WriteLine(ex.Message);
        outcome = MoveOutcome.GameIsOn;
        continue;
    }    
    RedrawField(gameControl.Grid);
} while (outcome == MoveOutcome.GameIsOn);
RedrawField(gameControl.Grid, true);

Console.WriteLine($"The result is: {outcome}.");

void RedrawField(Grid grid, bool showMines = false)
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
            if (grid.FieldGenerated(coordinate))
            {
                Field f = grid[coordinate];
                if (f.Player != null)
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
                        Console.Write(f.SurroundedBy);
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

Coordinate GetInput(Player player)
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
            if (row < rowMin || row > rowMax) continue;
            if (col < colMin || col > colMax) continue;
            return new(row, col);
        }
        catch
        {

        }
    }
    
}

Console.ReadKey();