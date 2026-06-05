using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.ViewModels
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public Game Game { get; set; } = null!;
        public string RankBadge => Rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"{Rank}🔹"
        };
    }
}
