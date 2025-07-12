namespace Zlebuh.MinTacToe.GameModel;

public class Field
{
    public Player? Player { get; set; }
    public int SurroundedByNotExplodedMines { get; set; }
    public bool IsMine { get; set; }
    public bool Generated { get; set; }
    public bool HasAllNeighboursGenerated { get; set; }
}
