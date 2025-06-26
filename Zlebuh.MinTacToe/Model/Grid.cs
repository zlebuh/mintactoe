namespace Zlebuh.MinTacToe.Model
{
    public class Grid : Dictionary<Coordinate, Field> 
    {
        public Grid MakeCopy()
        {
            Grid copy = [];
            foreach (var kvp in this)
            {
                copy[kvp.Key] = kvp.Value;
            }
            return copy;
        }
    }
}
