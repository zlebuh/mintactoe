using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.GameEngine.ModelExtensions;


internal static class MineGenerating
{
    public static Random random = new(); // can be mocked in tests
    public static void GenerateSafely(this Field field, Game game)
    {
        if (!field.Generated)
        {
            field.IsMine = game.GameState.MovesPlayed >= game.Rules.NoMineMoves && random.NextDouble() < game.Rules.MineProbability;
            field.Generated = true;
        }
    }

    public static void Generate(this Field field, Game game)
    {
        if (!field.Generated)
        {
            field.IsMine = random.NextDouble() < game.Rules.MineProbability;
            field.Generated = true;
        }
    }
}