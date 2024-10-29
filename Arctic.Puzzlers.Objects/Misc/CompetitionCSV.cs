namespace Arctic.Puzzlers.Objects.Misc
{
    ////date, player(s) name event name, piece count, rank, time, and remaining pieces
    public class CompetitionCSV
    {
        DateTime Date {  get; set; }
        string PlayersName { get; set; } = string.Empty;
        string EventName {  get; set; } = string.Empty;
        long PieceCount { get; set; }

        long Rank {  get; set; }
        TimeSpan Time { get; set; }

    }
}
