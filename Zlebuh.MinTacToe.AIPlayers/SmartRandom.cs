using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.AIPlayers
{
    public class SmartRandom : IAIPlayer
    {
        private readonly static (int, int)[] neighbours = new (int, int)[]
        {
            (1, 0), (0, 1), (-1, 0), (0, -1), (-1, 1), (1, 1), (1, -1), (-1, -1)
        };
        class CoordinateNode
        {
            private static Dictionary<CoordinateNode, int> Calculated = new();
            public CoordinateNode(Coordinate coordinate, bool isOccupied)
            {
                Coordinate = coordinate;
                IsOccupied = isOccupied;

            }
            public Coordinate Coordinate { get; }
            public bool IsOccupied { get; }
            public int GetLevel()
            {
                if (Calculated.ContainsKey(this))
                {
                    return Calculated[this];
                }
                if (IsOccupied)
                {
                    Calculated.Add(this, 0);
                    return 0;
                }
                int min = int.MaxValue;
                foreach (CoordinateNode node in Neighbours)
                {
                    int neighboursLevel = node.GetLevel();
                    if (neighboursLevel < min)
                    {
                        min = neighboursLevel;
                    }
                }
                Calculated.Add(this, min + 1);
                return min + 1;
            }
            public List<CoordinateNode> Neighbours { get; set; } = new();
        }

        private readonly HashSet<Coordinate> playableCoordinates = new();

        public Coordinate MakeMove(Game game, Player player)
        {
            List<Coordinate> changes = game.GameState.Changes;
            foreach (Coordinate change in changes)
            {
                foreach (Coordinate neighbour in GetCoordinateNeighbours(change, game.GameState.Grid))
                {
                    if (playableCoordinates.Contains(neighbour))
                    {
                        Field f = game.GameState.Grid[neighbour];
                        if (f.Player.HasValue)
                        {
                            playableCoordinates.Remove(neighbour);
                        }
                    }
                    else
                    {
                        playableCoordinates.Add(neighbour);
                    }
                }
            }
            

                if (game.GameState.MovesPlayed == 0)
                {
                    return new Coordinate(game.Rules.Rows / 2, game.Rules.Columns / 2);
                }


            Dictionary<Coordinate, CoordinateNode> nodes = new();
            for (int row = 0; row < game.Rules.Rows; row++)
            {
                for (int col = 0; col < game.Rules.Columns; col++)
                {
                    Coordinate c = new(row, col);
                    if (game.GameState.Grid.TryGetValue(c, out Field? value)) {
                        CoordinateNode node = new(c, value.Player.HasValue);
                        nodes[c] = node;
                    }
                }
            }

            Dictionary<int, List<Coordinate>> levels = new();
            foreach (var kvp in nodes)
            {
                if (kvp.Value.IsOccupied)
                {
                    continue;
                }
                int l = 0;
                foreach ((int rr, int cc) in neighbours)
                {
                    Coordinate n = new(kvp.Key.Row + rr, kvp.Key.Col + cc);
                    if (nodes.TryGetValue(n, out CoordinateNode? neighbour))
                    {
                        if (neighbour.IsOccupied)
                        {
                            l++;
                        }
                    }
                }
                if (!levels.TryGetValue(l, out List<Coordinate>? value))
                {
                    levels[l] = [kvp.Key];
                }
                else
                {
                    value.Add(kvp.Key);
                }
            }

            //Dictionary<int, List<Coordinate>> levels = nodes.GroupBy(n => n.Value.GetLevel()).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Select(a => a.Key).ToList());


            int min = levels.First().Key;
            int max = levels.Last().Key;

            Dictionary<int, double> levelProbs = new();

            double residue = 1;
            int levelRec = max;
            double ratio = 0.95;
            do
            {
                if (levelRec == min)
                {
                    levelProbs[min] = residue;
                    break;
                }
                else
                {
                    levelProbs[levelRec] = residue * ratio;
                    residue -= levelProbs[levelRec--];
                }
            } while (true);

            int level = GetRandomElementFromArrayWithProbabilities(levelProbs.Keys.ToArray(), [.. levelProbs.Values]);
            List<Coordinate> coordinatesToChooseFrom = levels[level];
            System.Random r = new();
            return coordinatesToChooseFrom[r.Next(coordinatesToChooseFrom.Count)];
        }

        public static T GetRandomElementFromArrayWithProbabilities<T>(T[] vals, double[] probs)
        {
            var vers = new double[probs.Length];
            double sum = probs.Sum();

            vers[0] = probs[0] / sum;
            for (int i = 1; i < vers.Length - 1; i++)
            {
                vers[i] = probs[i] / sum + vers[i - 1];
            }
            vers[vers.Length - 1] = 1.0;

            System.Random random = new();
            double rndval = random.NextDouble();
            for (int i = 0; i < vers.Length; i++)
                if (vers[i] >= rndval)
                    return vals[i];
            return vals.Last();
        }

        private readonly Dictionary<Coordinate, List<Coordinate>> coordinateToNeighboursCache = new();

        private List<Coordinate> GetCoordinateNeighbours(Coordinate coordinate, Grid grid)
        {
            if (coordinateToNeighboursCache.TryGetValue(coordinate, out List<Coordinate>? value))
            {
                return value;
            }
            else
            {
                List<Coordinate> res = new();
                foreach ((int rPan, int cPan) in neighbours)
                {
                    Coordinate neighbourCoordinate = new(coordinate.Row + rPan, coordinate.Col + cPan);
                    if (grid.ContainsKey(neighbourCoordinate))
                    {
                        res.Add(neighbourCoordinate);
                    }
                }
                coordinateToNeighboursCache[coordinate] = res;
                return res;
            }
        } 
    }
}
