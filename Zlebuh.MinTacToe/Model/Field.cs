namespace Zlebuh.MinTacToe.Model
{
    public class Field
    {
        internal Field(bool isMine)
        {
            IsMine = isMine;
        }
        public Player? Player { get; internal set; }
        public int SurroundedBy { get; internal set; } = 0;
        public bool IsMine { get; }
    }
}
