using SQLite;
using JournalApp.Models; 

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

  
    // Keep PrimaryMood/SecondaryMoods strings for backwards compatibility, but prefer relational mapping
    public string PrimaryMood { get; set; } = string.Empty; 
    public string SecondaryMoods { get; set; } = string.Empty; )

    // Tag relationship: a journal entry now references a single Tag by Id
    public int? TagId { get; set; }

    // Legacy tag storage (kept for compatibility)
    public string Tags { get; set; } = string.Empty; 

    // Add this property to fix the error
    public string Category { get; set; }

    // Compatibility helpers (not stored in DB)
    [Ignore]
    public string PrimaryMoodName => PrimaryMood ?? string.Empty;

    [Ignore]
    public string TagsString => Tags ?? string.Empty;

    [Ignore]
    public List<Tag> TagsList => (Tags ?? string.Empty)
        .Split(',')
        .Select(t => t.Trim())
        .Where(t => !string.IsNullOrEmpty(t))
        .Select(t => new Tag { Name = t })
        .ToList();

    [Ignore]
    public string SecondaryMoodsString => SecondaryMoods ?? string.Empty;
}
