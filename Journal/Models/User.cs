using SQLite;
using System;

namespace JournalApp.Models
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool HasPin { get; set; } = false;

        public string Theme { get; set; } = "Light";

        public string? CustomThemeJson { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }
    }
}