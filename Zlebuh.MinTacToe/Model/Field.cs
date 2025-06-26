namespace Zlebuh.MinTacToe.Model
{
    public class Field
    {
        public Player? Player { get; internal set; }
        public int SurroundedByNotExplodedMines { get; internal set; }
        public bool IsMine { get; internal set; }
        public bool Generated { get; set; }
        internal bool HasAllNeighboursGenerated { get; set; }
    }
}
