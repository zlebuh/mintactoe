using Zlebuh.MinTacToe.Services;

namespace Zlebuh.MinTacToe.Model
{
    public class Grid 
    {
        private readonly Dimension dimension;
        private readonly byte minePower;
        private readonly IMineAuthority mineAuthority;
        private readonly Dictionary<Coordinate, Field> fields = new();

        public Field this[Coordinate coordinate]
        {
            get
            {
                return fields[coordinate];
            }
            private set
            {
                fields[coordinate] = value;
            }
        }

        public bool FieldGenerated(Coordinate coordinate) => fields.ContainsKey(coordinate);

        internal Grid(Dimension dimension, IMineAuthority mineAuthority, byte minePower)
        {
            this.dimension = dimension;
            this.mineAuthority = mineAuthority;
            this.minePower = minePower;
        }

        internal Field GenerateMarked(Coordinate coordinate)
        {
            Field f;
            if (FieldGenerated(coordinate)) 
            {
                f = this[coordinate];
            }
            else
            {
                f = new(mineAuthority.IsMine(true));
                this[coordinate] = f;
            }
            f.SurroundedBy = 0;
            foreach ((int r, int c) in Directions)
            {
                Coordinate coor = new(coordinate.Row + r, coordinate.Col + c);
                if (dimension.CoordinateIsIn(coor))
                {
                    Field neighbour;
                    if (FieldGenerated(coor))
                    {
                        neighbour = this[coor];
                    }
                    else
                    {
                        neighbour = new(mineAuthority.IsMine(false));
                        this[coor] = neighbour;                        
                    }
                    if (neighbour.IsMine)
                    {
                        f.SurroundedBy++;
                    }
                }
            }
            return f;
        }

        internal static (int, int)[] Directions => new (int, int)[]
        {
            (1, 0),
            (0, 1),
            (-1, 0),
            (0, -1),
            (-1, 1),
            (1, 1),
            (1, -1),
            (-1, -1)
        };

        internal void Explode(Player player, Coordinate coordinate)
        {
            for (int i = coordinate.Row - minePower; i <= coordinate.Row + minePower; i++)
            {
                for (int j = coordinate.Col - minePower; j <= coordinate.Col + minePower; j++)
                {
                    Coordinate c = new(i, j);
                    if (dimension.CoordinateIsIn(c))
                    {
                        Field f = this[c];
                        if (f.Player == player)
                        {
                            f.Player = null;
                        }
                    }                    
                }
            }
        }
    }
}
