using System.Collections.Generic;

namespace JournalApp.Models
{
    public static class StaticData
    {
        public static List<Tag> GetAllTags()
        {
            return new List<Tag>
            {
                new Tag { Name = "Personal", Color = "#FFA726" },
                new Tag { Name = "Work", Color = "#29B6F6" },
                new Tag { Name = "Health", Color = "#66BB6A" },
                // ... add more defaults as you wish
            };
        }

        public static List<Mood> GetAllMoods() => new List<Mood>
        {
            new Mood { Name = "Happy", Category = "Primary", MoodType = "Positive" },
            new Mood { Name = "Sad", Category = "Primary", MoodType = "Negative" },
            new Mood { Name = "Calm", Category = "Secondary", MoodType = "Neutral" },
            // ...add more default moods as required
        };
    }
}