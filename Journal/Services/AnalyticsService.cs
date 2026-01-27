using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using JournalApp.Models;

namespace JournalApp.Services
{
    public class AnalyticsService
    {
        private readonly JournalService _journalService;

        public AnalyticsService(JournalService journalService)
        {
            _journalService = journalService;
        }

        public async Task<DashboardAnalytics> GetDashboardAnalyticsAsync()
        {
            var entries = await _journalService.GetEntriesAsync();

            var analytics = new DashboardAnalytics
            {
                TotalEntries = entries.Count,
                TotalWords = entries.Sum(e => GetWordCount(e.Content)),
                AverageWordCount = entries.Count > 0 ? Math.Round(entries.Sum(e => GetWordCount(e.Content)) * 1.0 / entries.Count, 1) : 0.0,
                MoodDistribution = GetMoodDistribution(entries),
                FrequentMoods = GetFrequentMoods(entries),
                MostUsedTags = GetMostUsedTags(entries),
                TagBreakdown = GetTagBreakdown(entries),
                WordCountTrends = GetWordCountTrends(entries, 30)
            };

            return analytics;
        }

        private int GetWordCount(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;
            var text = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", " ");
            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private List<MoodDistributionItem> GetMoodDistribution(List<JournalEntry> entries)
        {
            var moodGroups = entries.GroupBy(e => e.PrimaryMood)
                .Select(g => new MoodDistributionItem
                {
                    Mood = g.Key ?? "",
                    Count = g.Count(),
                    Percentage = entries.Count > 0 ? (g.Count() * 100.0) / entries.Count : 0.0
                })
                .OrderByDescending(m => m.Count)
                .ToList();
            return moodGroups;
        }

        private List<FrequentMood> GetFrequentMoods(List<JournalEntry> entries)
        {
            return entries.GroupBy(e => e.PrimaryMood)
                .Select(g => new FrequentMood
                {
                    Mood = g.Key ?? "",
                    Count = g.Count(),
                    Percentage = entries.Count > 0 ? Math.Round(g.Count() * 100.0 / entries.Count, 1) : 0.0
                })
                .OrderByDescending(m => m.Count)
                .ToList();
        }

        private List<TagUsage> GetMostUsedTags(List<JournalEntry> entries)
        {
            var tagDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Tags))
                {
                    var tags = entry.Tags.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t));
                    foreach (var tag in tags)
                    {
                        if (!tagDict.ContainsKey(tag))
                            tagDict[tag] = 0;
                        tagDict[tag]++;
                    }
                }
            }
            // Assign default color or expand as needed
            return tagDict.Select(t => new TagUsage
            {
                Name = t.Key,
                Count = t.Value,
                Color = "#7e57c2"
            })
                .OrderByDescending(t => t.Count)
                .ToList();
        }

        private List<TagUsage> GetTagBreakdown(List<JournalEntry> entries)
        {
            // Similar to above, but could be extended for other breakdowns
            return GetMostUsedTags(entries);
        }

        private List<WordCountTrend> GetWordCountTrends(List<JournalEntry> entries, int days)
        {
            var trends = new List<WordCountTrend>();
            var dateMap = entries
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => GetWordCount(e.Content)));

            var startDate = DateTime.Today.AddDays(-days + 1);
            for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
            {
                dateMap.TryGetValue(date, out int wordCount);
                trends.Add(new WordCountTrend { Date = date, WordCount = wordCount });
            }
            return trends;
        }
    }
}