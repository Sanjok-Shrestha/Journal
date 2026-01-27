using SQLite;

namespace JournalApp.Models
{
    public class Mood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string MoodType { get; set; } = ""; // e.g. Positive, Neutral, Negative
    }
}