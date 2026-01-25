using SQLite;
using System;

namespace JournalApp.Models
{
    [Table("Tags")]
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public string Color { get; set; } = "#3B82F6";

        public int UsageCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}