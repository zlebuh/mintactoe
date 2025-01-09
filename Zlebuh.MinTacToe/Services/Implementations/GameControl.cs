using Zlebuh.MinTacToe.Exceptions;
using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Services.Implementations
{
    internal class GameControl : IGameControl
    {
        private readonly Grid grid;
        private readonly IReferee referee;
        private Player playerOnMove;
        public Grid Grid => grid;
        public Player PlayerOnMove => playerOnMove;

        public GameControl(Grid grid, IReferee referee)
        {
            this.grid = grid;
            this.referee = referee;
            playerOnMove = Player.O;
        }

        public MoveOutcome PlaceMove(Player player, Coordinate coordinate)
        {
            Field field = grid.GenerateMarked(coordinate);
            if (field.Player.HasValue)
            {
                throw new FieldOccupiedException(field.Player.Value, coordinate);
            }
            if (field.IsMine)
            {
                grid.Explode(player, coordinate);
            }
            field.Player = player;
            MoveOutcome outcome = referee.CheckGrid(player, coordinate);
            playerOnMove = PlayerOnMove == Player.O ? Player.X : Player.O;
            return outcome;
        }
    }
}
