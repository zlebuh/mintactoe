using Zlebuh.MinTacToe.GameEngine;
using Zlebuh.MinTacToe.GameEngine.Exceptions;
using Zlebuh.MinTacToe.GameModel;
using Zlebuh.MinTacToe.GameSerialization;

namespace Zlebuh.MinTacToe.API.Services
{
    public interface IGameProxy
    {
        Task<string> CreateEmptySerializedGame();
        Task<(int errorCode, string message, string? gameState)> MakeAMove(string gameState, Coordinate coordinate, Player player);
    }

    internal class GameProxy(ISerializer serializer) : IGameProxy
    {
        private static readonly Rules Rules = new() { Rows = 16, Columns = 16 };
        private readonly ISerializer serializer = serializer;

        public async Task<string> CreateEmptySerializedGame()
        {
            Game game = GameControl.Initialize(Rules);
            string serialized = await serializer.SerializeGameAsync(game);
            return serialized;
        }

        public async Task<(int errorCode, string message, string? gameState)> MakeAMove(string gameState, Coordinate coordinate, Player player)
        {
            Game game = await serializer.DeserializeGameAsync(gameState);
            try
            {
                GameControl.MakeMove(game, player, coordinate);
                string serializedGame = await serializer.SerializeGameAsync(game);
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
