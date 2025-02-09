using Zlebuh.MinTacToe.Model;

namespace Zlebuh.MinTacToe.AIPlayers
{
    public class MonteCarlo : IAIPlayer
    {
        public Coordinate MakeMove(Game game, Player player)
        {
            if (game.GameState.MovesPlayed == 0)
            {
                return new Coordinate(game.Rules.Rows / 2, game.Rules.Columns / 2);
            }

            return GetMCTSMove(game);
        }

        public Coordinate GetMCTSMove(Game game, int timeLimitMs = 5000)
        {
            MCTSNode root = new(); // Kořenový uzel
            long startTime = DateTime.UtcNow.Ticks;

            while ((DateTime.UtcNow.Ticks - startTime) / TimeSpan.TicksPerMillisecond < timeLimitMs)
            {
                MCTSNode promisingNode = SelectPromisingNode(root);
                if (promisingNode.Visits > 0) ExpandNode(promisingNode, game);
                MCTSNode nodeToSimulate = promisingNode.Children.Count > 0 ? promisingNode.Children[0] : promisingNode;
                int result = SimulateRandomGame(game, game.GameState.PlayerOnTurn!.Value);
                Backpropagate(nodeToSimulate, result);
            }

            return root.BestChild().Coordinate;
        }

        private void Backpropagate(MCTSNode node, int result)
        {
            while (node != null)
            {
                node.Visits++;
                if (result == 1) node.Wins++; // AI vyhrála
                node = node.Parent;
            }
        }

        private int SimulateRandomGame(Game game, Player aiPlayer)
        {
            Game copiedGame = game.MakeCopy();
            System.Random rand = new();
            while (!copiedGame.GameState.IsGameOver) // Dokud hra nekončí
            {
                List<Coordinate> emptyCells = new();
                for (int x = 0; x < copiedGame.Rules.Rows; x++)
                {
                    for (int y = 0; y < copiedGame.Rules.Columns; y++)
                    {
                        Coordinate coordinate = new(x, y);
                        Field f = copiedGame.GameState.Grid[coordinate];
                        if (!f.Player.HasValue)
                        {
                            emptyCells.Add(coordinate);
                        }
                    }
                }

                if (emptyCells.Count == 0) return 0; // Remíza

                Coordinate selected = emptyCells[rand.Next(emptyCells.Count)];
                if (!copiedGame.GameState.PlayerOnTurn.HasValue)
                {
                    return 0;
                }
                GameControl.MakeMove(copiedGame, copiedGame.GameState.PlayerOnTurn.Value, selected);
            }

            return copiedGame.GameState.Winner.HasValue ? copiedGame.GameState.Winner == aiPlayer ? 1 : -1 : 0;

            throw new NotImplementedException();
        }

        private void ExpandNode(MCTSNode node, Game game)
        {
            for (int row = 0; row < game.Rules.Rows; row++)
            {
                for (int col = 0; col < game.Rules.Columns; col++)
                {
                    Coordinate coordinate = new(row, col);
                    Field f = game.GameState.Grid[coordinate];
                    if (!f.Player.HasValue) // Prázdné políčko
                    {
                        node.Children.Add(new MCTSNode(new(row, col), node));
                    }
                }
            }
        }

        private MCTSNode SelectPromisingNode(MCTSNode node)
        {
            while (node.Children.Count > 0)
            {
                node = node.Children.OrderByDescending(n => n.UCT()).First();
            }
            return node;
        }
    }

    public class MCTSNode
    {
        public Coordinate Coordinate; // Souřadnice tahu
        public int Visits; // Počet simulací
        public double Wins; // Počet výher
        public List<MCTSNode> Children; // Děti (možné tahy)
        public MCTSNode Parent; // Odkaz na rodiče

        public MCTSNode(Coordinate? coordinate = null, MCTSNode? parent = null)
        {
            if (coordinate.HasValue)
            {
                Coordinate = coordinate.Value;
            }
            Wins = 0;
            Visits = 0;
            Parent = parent!;
            Children = new List<MCTSNode>();
        }

        public double UCT()
        {
            if (Visits == 0) return double.MaxValue; // Pro neprozkoumané tahy
            return (Wins / Visits) + 1.41 * Math.Sqrt(Math.Log(Parent.Visits) / Visits);
        }

        public MCTSNode BestChild()
        {
            return Children.OrderByDescending(c => c.Visits).First();
        }
    }
}