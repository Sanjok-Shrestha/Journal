using SQLite;

namespace JournalApp.Models;

[Table("JournalEntryTags")]
public class JournalEntryTag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int JournalEntryId { get; set; }

    public int TagId { get; set; }
}