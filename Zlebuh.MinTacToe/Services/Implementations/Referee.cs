using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.Services.Implementations
{
    internal class Referee : IReferee
    {
        private readonly Grid grid;
        private readonly byte seriesLength;
        private readonly Dimension dimension;

        public Referee(Grid grid, byte seriesLength, Dimension dimension)
        {
            this.grid = grid;
            this.seriesLength = seriesLength;
            this.dimension = dimension;
        }
        public MoveOutcome CheckGrid(Player movePlacedByPlayer, Coordinate lastPlacedCoordinate)
        {
            foreach ((int r, int c) in Grid.Directions)
            {
                int rc = lastPlacedCoordinate.Row;
                int cc = lastPlacedCoordinate.Col;
                int counter = 1;
                do
                {
                    rc += r;
                    cc += c;

                    Coordinate coor = new(rc, cc);
                    if (!dimension.CoordinateIsIn(coor)) break;
                    if (!grid.FieldGenerated(coor)) break;
                    Field f = grid[coor];
                    if (f.Player == movePlacedByPlayer)
                    {
                        counter++;
                        if (counter == seriesLength) return movePlacedByPlayer == Player.O ? MoveOutcome.OWins : MoveOutcome.XWins;
                    }
                    else
                    {
                        break;
                    }
                } while (true);

            }

            return MoveOutcome.GameIsOn;
        }
    }
}
