using SQLite;
using JournalApp.Models;

namespace JournalApp.Data
{
    public class JournalDbContext
    {
        private readonly SQLiteAsyncConnection _database;

        public JournalDbContext()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTableAsync<User>().Wait();
            _database.CreateTableAsync<JournalEntry>().Wait();
            _database.CreateTableAsync<Tag>().Wait();
        }

        // Journal Entry Methods
        public Task<List<JournalEntry>> GetEntriesAsync(int userId)
        {
            return _database.Table<JournalEntry>()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        public Task<JournalEntry?> GetEntryByDateAsync(int userId, DateTime date)
        {
            var targetDate = date.Date;
            return _database.Table<JournalEntry>()
                .Where(e => e.UserId == userId && e.EntryDate == targetDate)
                .FirstOrDefaultAsync();
        }

        public Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            return _database.Table<JournalEntry>()
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            entry.WordCount = entry.Content
                .Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;

            if (entry.Id != 0)
            {
                entry.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(entry);
            }
            else
            {
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                return await _database.InsertAsync(entry);
            }
        }

        public Task<int> DeleteEntryAsync(JournalEntry entry)
        {
            return _database.DeleteAsync(entry);
        }

        // User Methods
        public Task<User?> GetUserByUsernameAsync(string username)
        {
            return _database.Table<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public Task<User?> GetUserByIdAsync(int id)
        {
            return _database.Table<User>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public Task<int> SaveUserAsync(User user)
        {
            if (user.Id != 0)
            {
                return _database.UpdateAsync(user);
            }
            else
            {
                user.CreatedAt = DateTime.Now;
                return _database.InsertAsync(user);
            }
        }

        // Tag Methods
        public Task<List<Tag>> GetTagsAsync(int userId)
        {
            return _database.Table<Tag>()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.UsageCount)
                .ToListAsync();
        }

        public Task<int> SaveTagAsync(Tag tag)
        {
            if (tag.Id != 0)
            {
                return _database.UpdateAsync(tag);
            }
            else
            {
                tag.CreatedAt = DateTime.Now;
                return _database.InsertAsync(tag);
            }
        }

        public Task<Tag?> GetTagByNameAsync(int userId, string name)
        {
            return _database.Table<Tag>()
                .Where(t => t.UserId == userId && t.Name == name)
                .FirstOrDefaultAsync();
        }
    }
}