using Zlebuh.MinTacToe.GameModel;

namespace Zlebuh.MinTacToe.UI.Shared
{
    public static class PlayerExtension
    {
        public static string ToDisplayString(this Player? player)
        {
            return player switch
            {
                Player.X => "red",
                Player.O => "blue",
                _ => "no one"
            };
        }

        public static string ToColor(this Player? player)
        {
            return player switch
            {
                Player.X => "#ff0064",
                Player.O => "#00c8e1",
                _ => "transparent"
            };
        }
    }
}
