namespace Zlebuh.MinTacToe.GameModel;

public class Grid : Dictionary<Coordinate, Field>
{
    public Grid MakeCopy()
    {
        Grid copy = [];
        foreach (KeyValuePair<Coordinate, Field> kvp in this)
        {
            copy[kvp.Key] = kvp.Value;
        }
        return copy;
    }
}
