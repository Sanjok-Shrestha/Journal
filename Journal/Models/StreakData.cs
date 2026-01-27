namespace JournalApp.Models;

public class StreakData
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalEntries { get; set; }
    public int MissedDays { get; set; }
}