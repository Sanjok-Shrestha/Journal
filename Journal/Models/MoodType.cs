namespace JournalApp.Models
{
    public enum MoodType
    {
        Happy,
        Sad,
        Angry,
        Anxious,
        Excited,
        Calm,
        Stressed,
        Grateful,
        Lonely,
        Confident,
        Bored,
        Nostalgic
    }

    public static class MoodExtensions
    {
        public static string GetEmoji(this MoodType mood)
        {
            return mood switch
            {
                MoodType.Happy => "Happy",
                MoodType.Sad => "Sad",
                MoodType.Angry => "Angry",
                MoodType.Anxious => "Anxious",
                MoodType.Excited => "Excited",
                MoodType.Calm => "Calm",
                MoodType.Stressed => "Stressed",
                MoodType.Grateful => "Grateful",
                MoodType.Lonely => "Lonely",
                MoodType.Confident => "Confident",
                MoodType.Bored => "Bored",
                MoodType.Nostalgic => "Nostalgic"
            };
        }

        public static string GetColor(this MoodType mood)
        {
            return mood switch
            {
                MoodType.Happy => "#FFD700",
                MoodType.Sad => "#4682B4",
                MoodType.Angry => "#FF4500",
                MoodType.Anxious => "#FF6347",
                MoodType.Excited => "#FF69B4",
                MoodType.Calm => "#87CEEB",
                MoodType.Stressed => "#FF8C00",
                MoodType.Grateful => "#32CD32",
                MoodType.Lonely => "#708090",
                MoodType.Confident => "#FFD700",
                MoodType.Bored => "#A9A9A9",
                MoodType.Nostalgic => "#DDA0DD"
            };
        }
    }
}