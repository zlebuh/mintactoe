using Zlebuh.MinTacToe.Exceptions;
using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.API.Services
{
    public interface IGameProxy
    {
        Task<string> CreateEmptySerializedGame();
        Task<(int errorCode, string message, string? gameState)> MakeAMove(string gameState, Coordinate coordinate, Player player);
    }

    internal class GameProxy : IGameProxy
    {
        private static readonly Rules Rules = new() { Rows = 16, Columns = 16 };
        public async Task<string> CreateEmptySerializedGame()
        {
            Game game = GameControl.Initialize(Rules);
            string serialized = await GameSerializer.SerializeGame(game);
            return serialized;
        }

        public async Task<(int errorCode, string message, string? gameState)> MakeAMove(string gameState, Coordinate coordinate, Player player)
        {
            Game game = await GameSerializer.DeserializeGame(gameState);
            try
            {
                GameControl.MakeMove(game, player, coordinate);
                string serializedGame = await GameSerializer.SerializeGame(game);
                return (0, "Move made successfully.", serializedGame);
            }
            catch (GameIsOverException)
            {
                return (1, "The game is already over.", null);
            }
            catch (NotYourTurnException ex)
            {
                return (2, $"It's not your turn. Current player: {ex.PlayerOnTurn}", null);
            }
            catch (CoordinateOutOfGridException ex)
            {
                return (3, $"Coordinate {ex.Coordinate} is out of grid bounds. Rows: {ex.Rows}, Columns: {ex.Columns}", null);
            }
            catch (FieldOccupiedException ex)
            {
                return (4, $"Field at {ex.OccupiedCoordinate} is already occupied by player {ex.OccupyingPlayer}.", null);
            }
            catch (Exception ex)
            {
                return (5, $"An unexpected error occurred: {ex.Message}", null);
            }
        }
    }
}
