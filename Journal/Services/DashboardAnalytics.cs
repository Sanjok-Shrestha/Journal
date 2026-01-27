using System;
using System.Collections.Generic;

namespace JournalApp.Models
{
    public class DashboardAnalytics
    {
        public int TotalEntries { get; set; }
        public int TotalWords { get; set; }
        public double AverageWordCount { get; set; }
        public List<MoodDistributionItem> MoodDistribution { get; set; } = new();
        public List<FrequentMood> FrequentMoods { get; set; } = new();
        public List<TagUsage> MostUsedTags { get; set; } = new();
        public List<TagUsage> TagBreakdown { get; set; } = new();
        public List<WordCountTrend> WordCountTrends { get; set; } = new();
    }

    public class MoodDistributionItem
    {
        public string Mood { get; set; } = "";
        public int Count { get; set; }
        public double Percentage { get; set; }

        public string GetEmoji() => Mood switch
        {
            "Happy" => "?",
            "Sad" => "?",
            "Angry" => "?",
            "Calm" => "?",
            "Excited" => "",
            _ => ""
        };

        public string GetColor() => Mood switch
        {
            "Happy" => "#FFD700",
            "Sad" => "#90CAF9",
            "Angry" => "#FF7043",
            "Calm" => "#80CBC4",
            "Excited" => "#FFB300",
            _ => "#BDBDBD"
        };
    }

    public class FrequentMood
    {
        public string Mood { get; set; } = "";
        public int Count { get; set; }
        public double Percentage { get; set; }

        public string GetEmoji() => new MoodDistributionItem { Mood = this.Mood }.GetEmoji();
        public string GetColor() => new MoodDistributionItem { Mood = this.Mood }.GetColor();
    }

    public class TagUsage
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public string Color { get; set; } = "#AAA"; // optional
    }

    public class WordCountTrend
    {
        public DateTime Date { get; set; }
        public int WordCount { get; set; }
    }
}