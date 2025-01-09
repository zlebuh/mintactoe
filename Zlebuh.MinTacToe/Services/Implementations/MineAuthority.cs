namespace Zlebuh.MinTacToe.Services.Implementations
{
    internal class MineAuthority : IMineAuthority
    {
        private readonly double mineProbability;
        private readonly short noMineMovesTotal;
        private short moveCounter = 0;
        private readonly Random random;

        public MineAuthority(double mineProbability, byte noMineMoves)
        {
            this.mineProbability = mineProbability;
            noMineMovesTotal = (short)(noMineMoves * 2);
            random = new();

        }
        public bool IsMine(bool considerNoMineMovesRule)
        {
            if (moveCounter <= noMineMovesTotal)
            {
                moveCounter++;
                return false;
            }
            return random.NextDouble() < mineProbability;
        }
    }
}
