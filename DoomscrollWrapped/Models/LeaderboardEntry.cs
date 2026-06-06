namespace DoomscrollWrapped.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }

        public string Nickname { get; set; } = string.Empty;

        public int TotalMinutes { get; set; }

        public string RankDisplay => Rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"{Rank}."
        };

        public string HoursText => $"{System.Math.Round(TotalMinutes / 60.0, 1)} hrs";
    }
}
