namespace Arctic.Puzzlers.Objects.Misc
{
    ////date, player(s) name event name, piece count, rank, time, and remaining pieces
    public class CompetitionCSV
    {
        public DateTime Date {  get; set; }
        public string PlayersName { get; set; } = string.Empty;
        public string EventName {  get; set; } = string.Empty;
        public long PieceCount { get; set; }

        public long Rank {  get; set; }
        public TimeSpan Time { get; set; }
    }
}
