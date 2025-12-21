using System;
using System.Collections.Generic;

namespace Journal.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Today;
        public string Content { get; set; } = string.Empty;

        public MoodType PrimaryMood { get; set; }
        public MoodType? SecondaryMood1 { get; set; }
        public MoodType? SecondaryMood2 { get; set; }

        public List<Tag> Tags { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
