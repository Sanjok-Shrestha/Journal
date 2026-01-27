using SQLite;

namespace JournalApp.Models;

[Table("JournalEntries")]
public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string PrimaryMood { get; set; } = string.Empty;

    public string SecondaryMoods { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;
}