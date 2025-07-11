using System.Text.Json.Serialization;

namespace Zlebuh.MinTacToe.Model
{
    public class Field
    {
        [JsonInclude]
        public Player? Player { get; internal set; }
        [JsonInclude]
        public int SurroundedByNotExplodedMines { get; internal set; }
        [JsonInclude]
        public bool IsMine { get; internal set; }
        [JsonInclude]
        public bool Generated { get; set; }
        [JsonInclude]
        internal bool HasAllNeighboursGenerated { get; set; }
    }
}
