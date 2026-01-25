using JournalApp.Data;
using JournalApp.Models;

namespace JournalApp.Services
{
    public class MoodDistribution
    {
        public MoodType Mood { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TagStatistics
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class WordCountTrend
    {
        public DateTime Date { get; set; }
        public int WordCount { get; set; }
    }

    public class DashboardAnalytics
    {
        public List<MoodDistribution> MoodDistribution { get; set; } = new();
        public List<MoodDistribution> FrequentMoods { get; set; } = new();
        public List<TagStatistics> MostUsedTags { get; set; } = new();
        public List<TagStatistics> TagBreakdown { get; set; } = new();
        public List<WordCountTrend> WordCountTrends { get; set; } = new();
        public int TotalEntries { get; set; }
        public int AverageWordCount { get; set; }
        public int TotalWords { get; set; }
    }

    public interface IAnalyticsService
    {
        Task<DashboardAnalytics> GetDashboardAnalyticsAsync();
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly IJournalService _journalService;
        private readonly JournalDbContext _context;
        private readonly IAuthenticationService _authService;

        public AnalyticsService(IJournalService journalService, JournalDbContext context, IAuthenticationService authService)
        {
            _journalService = journalService;
            _context = context;
            _authService = authService;
        }

        public async Task<DashboardAnalytics> GetDashboardAnalyticsAsync()
        {
            var entries = await _journalService.GetAllEntriesAsync();
            var user = await _authService.GetCurrentUserAsync();
            var tags = await _context.GetTagsAsync(user?.Id ?? 0);

            var analytics = new DashboardAnalytics();

            if (!entries.Any())
                return analytics;

            var allMoods = new List<MoodType>();
            foreach (var entry in entries)
            {
                allMoods.Add(entry.PrimaryMood);
                if (entry.SecondaryMood1.HasValue)
                    allMoods.Add(entry.SecondaryMood1.Value);
                if (entry.SecondaryMood2.HasValue)
                    allMoods.Add(entry.SecondaryMood2.Value);
            }

            var moodCounts = allMoods
                .GroupBy(m => m)
                .Select(g => new MoodDistribution
                {
                    Mood = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((double)g.Count() / allMoods.Count * 100, 2)
                })
                .OrderByDescending(m => m.Count)
                .ToList();

            analytics.MoodDistribution = moodCounts;
            analytics.FrequentMoods = moodCounts.Take(5).ToList();

            analytics.TagBreakdown = tags.Select(t => new TagStatistics
            {
                Name = t.Name,
                Count = t.UsageCount,
                Color = t.Color
            }).ToList();

            analytics.MostUsedTags = analytics.TagBreakdown.Take(5).ToList();

            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            analytics.WordCountTrends = entries
                .Where(e => e.EntryDate >= thirtyDaysAgo)
                .OrderBy(e => e.EntryDate)
                .Select(e => new WordCountTrend
                {
                    Date = e.EntryDate,
                    WordCount = e.WordCount
                })
                .ToList();

            analytics.TotalEntries = entries.Count;
            analytics.TotalWords = entries.Sum(e => e.WordCount);
            analytics.AverageWordCount = entries.Any() ? (int)entries.Average(e => e.WordCount) : 0;

            return analytics;
        }
    }
}