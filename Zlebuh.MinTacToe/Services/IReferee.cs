using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Services
{
    internal interface IReferee
    {
        MoveOutcome CheckGrid(Player movePlacedByPlayer, Coordinate lastPlacedCoordinate);
    }
}
