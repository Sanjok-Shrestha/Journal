using JournalApp.Models;

namespace JournalApp.Services;

public static class StaticData
{
    public static List<Mood> GetAllMoods()
    {
        return new List<Mood>
        {
            // Positive (MoodType = Positive)
            // Mark one as Primary and others Secondary
            new Mood { Name = "Happy", Category = "Primary", MoodType = "Positive" },
            new Mood { Name = "Excited", Category = "Secondary", MoodType = "Positive" },
            new Mood { Name = "Relaxed", Category = "Secondary", MoodType = "Positive" },
            new Mood { Name = "Grateful", Category = "Secondary", MoodType = "Positive" },
            new Mood { Name = "Confident", Category = "Secondary", MoodType = "Positive" },

            // Neutral (MoodType = Neutral)
            new Mood { Name = "Calm", Category = "Primary", MoodType = "Neutral" },
            new Mood { Name = "Thoughtful", Category = "Secondary", MoodType = "Neutral" },
            new Mood { Name = "Curious", Category = "Secondary", MoodType = "Neutral" },
            new Mood { Name = "Nostalgic", Category = "Secondary", MoodType = "Neutral" },
            new Mood { Name = "Bored", Category = "Secondary", MoodType = "Neutral" },

            // Negative (MoodType = Negative)
            new Mood { Name = "Sad", Category = "Primary", MoodType = "Negative" },
            new Mood { Name = "Angry", Category = "Secondary", MoodType = "Negative" },
            new Mood { Name = "Stressed", Category = "Secondary", MoodType = "Negative" },
            new Mood { Name = "Lonely", Category = "Secondary", MoodType = "Negative" },
            new Mood { Name = "Anxious", Category = "Secondary", MoodType = "Negative" },
            new Mood { Name = "Tired", Category = "Secondary", MoodType = "Negative" }
        };
    }

    public static List<Tag> GetAllTags()
    {
        return new List<Tag>
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
            new Tag { Name = "Victpory" }
        };
    }

    public static bool IsPositiveMood(string? mood)
    {
        if (string.IsNullOrEmpty(mood)) return false;
        return GetAllMoods().Where(m => m.MoodType == "Positive").Select(m => m.Name).Contains(mood);
    }

    public static bool IsNeutralMood(string? mood)
    {
        if (string.IsNullOrEmpty(mood)) return false;
        return GetAllMoods().Where(m => m.MoodType == "Neutral").Select(m => m.Name).Contains(mood);
    }

    public static bool IsNegativeMood(string? mood)
    {
        if (string.IsNullOrEmpty(mood)) return false;
        return GetAllMoods().Where(m => m.MoodType == "Negative").Select(m => m.Name).Contains(mood);
    }
}