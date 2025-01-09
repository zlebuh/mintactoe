namespace Zlebuh.MinTacToe.Model
{
    public class Rules
    {
        public byte Rows { get; set; } = 20;
        public byte Columns { get; set; } = 20;
        public byte SeriesLength { get; set; } = 5;
        public byte NoMineMoves { get; set; } = 3;
        public byte MinePower { get; set; } = 1;
        public double MineProbability { get; set; } = 0.1;
        //public TimePolicy TimePolicy { get; set; } = TimePolicy.Unlimited; // todo 
    }    
}
