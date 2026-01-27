using SQLite;

namespace JournalApp.Models
{
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Color { get; set; } // optional
    }
}