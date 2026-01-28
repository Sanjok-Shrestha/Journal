using System;
using System.Collections.Generic;
using System.Linq;
using JournalApp.Models;

namespace JournalApp.Services
{
    /// <summary>
    /// Provides application-wide static lookup data used as a fallback or initial seed for the database.
    /// The lists are cached in-memory to avoid reallocating new lists on every call.
    /// </summary>
    public static class StaticData
    {
        // Backing lists are initialized once and reused. This improves performance compared to
        // creating a new list each time GetAllMoods/GetAllTags is called.
        private static readonly List<Mood> _moods;
        private static readonly List<Tag> _tags;

        // Precomputed lookup sets for fast, case-insensitive membership checks.
        private static readonly HashSet<string> _positiveMoodNames;
        private static readonly HashSet<string> _neutralMoodNames;
        private static readonly HashSet<string> _negativeMoodNames;

        // Static constructor to initialize data and lookup sets once.
        static StaticData()
        {
            _moods = new List<Mood>
            {
                // Primary moods are intended as main selections; Secondary are allowed as supporting moods.
                // MoodType categorizes each mood into Positive / Neutral / Negative for analytics.
                new Mood { Name = "Happy",      Category = "Primary",   MoodType = "Positive" },
                new Mood { Name = "Excited",    Category = "Secondary", MoodType = "Positive" },
                new Mood { Name = "Relaxed",    Category = "Secondary", MoodType = "Positive" },
                new Mood { Name = "Grateful",   Category = "Secondary", MoodType = "Positive" },
                new Mood { Name = "Confident",  Category = "Secondary", MoodType = "Positive" },

                new Mood { Name = "Calm",       Category = "Primary",   MoodType = "Neutral" },
                new Mood { Name = "Thoughtful", Category = "Secondary", MoodType = "Neutral" },
                new Mood { Name = "Curious",    Category = "Secondary", MoodType = "Neutral" },
                new Mood { Name = "Nostalgic",  Category = "Secondary", MoodType = "Neutral" },
                new Mood { Name = "Bored",      Category = "Secondary", MoodType = "Neutral" },

                new Mood { Name = "Sad",        Category = "Primary",   MoodType = "Negative" },
                new Mood { Name = "Angry",      Category = "Secondary", MoodType = "Negative" },
                new Mood { Name = "Stressed",   Category = "Secondary", MoodType = "Negative" },
                new Mood { Name = "Lonely",     Category = "Secondary", MoodType = "Negative" },
                new Mood { Name = "Anxious",    Category = "Secondary", MoodType = "Negative" },
                new Mood { Name = "Tired",      Category = "Secondary", MoodType = "Negative" }
            };

            _tags = new List<Tag>
            {
                new Tag { Name = "Work" },
                new Tag { Name = "Career" },
                new Tag { Name = "Studies" },
                new Tag { Name = "Family" },
                new Tag { Name = "Friends" },
                new Tag { Name = "Relationships" },
                new Tag { Name = "Parenting" },
                new Tag { Name = "Health" },
                new Tag { Name = "Fitness" },
                new Tag { Name = "Personal Growth" },
                new Tag { Name = "Self-care" },
                new Tag { Name = "Exercise" },
                new Tag { Name = "Meditation" },
                new Tag { Name = "Yoga" },
                new Tag { Name = "Hobbies" },
                new Tag { Name = "Travel" },
                new Tag { Name = "Nature" },
                new Tag { Name = "Reading" },
                new Tag { Name = "Shopping" },
                new Tag { Name = "Cooking" },
                new Tag { Name = "Music" },
                new Tag { Name = "Writing" },
                new Tag { Name = "Finance" },
                new Tag { Name = "Projects" },
                new Tag { Name = "Planning" },
                new Tag { Name = "Spirituality" },
                new Tag { Name = "Reflection" },
                new Tag { Name = "Birthday" },
                new Tag { Name = "Holiday" },
                new Tag { Name = "Vacation" },
                // fixed typo: "Victory" instead of "Victpory"
                new Tag { Name = "Victory" }
            };

            // Build fast lookup sets (case-insensitive)
            _positiveMoodNames = new HashSet<string>(_moods.Where(m => string.Equals(m.MoodType, "Positive", StringComparison.OrdinalIgnoreCase))
                                                          .Select(m => m.Name),
                                                     StringComparer.OrdinalIgnoreCase);

            _neutralMoodNames = new HashSet<string>(_moods.Where(m => string.Equals(m.MoodType, "Neutral", StringComparison.OrdinalIgnoreCase))
                                                         .Select(m => m.Name),
                                                    StringComparer.OrdinalIgnoreCase);

            _negativeMoodNames = new HashSet<string>(_moods.Where(m => string.Equals(m.MoodType, "Negative", StringComparison.OrdinalIgnoreCase))
                                                          .Select(m => m.Name),
                                                     StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the statically defined moods. The returned list is a shared instance (read-only usage is recommended).
        /// </summary>
        public static IReadOnlyList<Mood> GetAllMoods() => _moods;

        /// <summary>
        /// Returns the statically defined tags.
        /// </summary>
        public static IReadOnlyList<Tag> GetAllTags() => _tags;

        /// <summary>
        /// Returns true when the provided mood name is classified as a positive mood.
        /// Comparison is case-insensitive and tolerant of leading/trailing whitespace.
        /// </summary>
        public static bool IsPositiveMood(string? mood)
        {
            if (string.IsNullOrWhiteSpace(mood)) return false;
            return _positiveMoodNames.Contains(mood.Trim());
        }

        /// <summary>
        /// Returns true when the provided mood name is classified as a neutral mood.
        /// Comparison is case-insensitive and tolerant of leading/trailing whitespace.
        /// </summary>
        public static bool IsNeutralMood(string? mood)
        {
            if (string.IsNullOrWhiteSpace(mood)) return false;
            return _neutralMoodNames.Contains(mood.Trim());
        }

        /// <summary>
        /// Returns true when the provided mood name is classified as a negative mood.
        /// Comparison is case-insensitive and tolerant of leading/trailing whitespace.
        /// </summary>
        public static bool IsNegativeMood(string? mood)
        {
            if (string.IsNullOrWhiteSpace(mood)) return false;
            return _negativeMoodNames.Contains(mood.Trim());
        }

        /// <summary>
        /// Helper: gets mood names for a given MoodType (Positive/Neutral/Negative).
        /// Returns an empty list for unknown types.
        /// </summary>
        public static IReadOnlyList<string> GetMoodNamesByType(string moodType)
        {
            if (string.IsNullOrWhiteSpace(moodType)) return Array.Empty<string>();

            moodType = moodType.Trim();
            return moodType.Equals("Positive", StringComparison.OrdinalIgnoreCase) ? _positiveMoodNames.ToList()
                 : moodType.Equals("Neutral", StringComparison.OrdinalIgnoreCase) ? _neutralMoodNames.ToList()
                 : moodType.Equals("Negative", StringComparison.OrdinalIgnoreCase) ? _negativeMoodNames.ToList()
                 : Array.Empty<string>();
        }
    }
}
