using SQLite;

namespace JournalApp.Models;

[Table("Moods")]
public class Mood
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // User-facing label for the mood (e.g., "Happy", "Sad")
    public string Name { get; set; } = string.Empty;

    // Sentiment category: "Positive", "Neutral", or "Negative" (used for analytics and filtering)
    public string Category { get; set; } = string.Empty;

    // Optional subtype or grouping key (e.g., same as Name or a broader group like "All")
    // Useful for UI grouping or advanced filtering scenarios
    public string MoodType { get; set; } = string.Empty;
}