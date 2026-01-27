using System;
using SQLite;

namespace JournalApp.Models;

[Table("JournalEntries")]
public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    // Optional: A backwards-compatible alias, NOT a duplicate!
    [Ignore]
    [Obsolete("Use Date property instead of EntryDate.", false)]
    public DateTime EntryDate { get; set; }

    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string PrimaryMood { get; set; } = "";
    public string SecondaryMoods { get; set; } = "";
    public string Category { get; set; } = "";
    public string Tags { get; set; } = "";
    public int WordCount { get; set; } = 0;
}