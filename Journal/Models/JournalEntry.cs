using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SQLite;

namespace JournalApp.Models
{
    [Table("JournalEntries")]
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, SQLite.MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        public MoodType PrimaryMood { get; set; }

        public MoodType? SecondaryMood1 { get; set; }

        public MoodType? SecondaryMood2 { get; set; }

        public string Tags { get; set; } = string.Empty;

        public int WordCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Ignore]
        public List<string> TagList
        {
            get => string.IsNullOrEmpty(Tags)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
            set => Tags = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}