using SQLite;
using JournalApp.Models;

namespace JournalApp.Services;

public class JournalService
{
    private readonly SQLiteAsyncConnection _database;

    public JournalService(string dbPath)
    {
        try
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<JournalEntry>().Wait();
            Console.WriteLine($"Database initialized at: {dbPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetEntriesAsync()
    {
        try
        {
            return await _database.Table<JournalEntry>()
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetEntriesAsync error: {ex.Message}");
            return new List<JournalEntry>();
        }
    }

    public async Task<JournalEntry?> GetEntryAsync(int id)
    {
        try
        {
            return await _database.Table<JournalEntry>()
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetEntryAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<JournalEntry>> GetEntriesByDateAsync(DateTime date)
    {
        try
        {
            // Use range comparison to ensure compatibility with SQLite translation
            var start = date.Date;
            var end = start.AddDays(1).AddTicks(-1);

            return await _database.Table<JournalEntry>()
                .Where(e => e.Date >= start && e.Date <= end)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetEntriesByDateAsync error: {ex.Message}");
            return new List<JournalEntry>();
        }
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        try
        {
            var start = date.Date;
            var end = start.AddDays(1).AddTicks(-1);
            return await _database.Table<JournalEntry>()
                .Where(e => e.Date >= start && e.Date <= end)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetEntryByDateAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<int> SaveEntryAsync(JournalEntry entry)
    {
        try
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (entry.Id != 0)
            {
                // Existing entry - update
                entry.UpdatedAt = DateTime.Now;
                var result = await _database.UpdateAsync(entry);
                Console.WriteLine($"Entry updated. ID: {entry.Id}, Result: {result}");
                return result;
            }
            else
            {
                // New entry - enforce single entry per day
                var existing = await GetEntryByDateAsync(entry.Date == default ? DateTime.Today : entry.Date);
                if (existing != null)
                {
                    // If an entry already exists for the date, update that entry instead of inserting a duplicate
                    entry.Id = existing.Id;
                    // Preserve original CreatedAt
                    entry.CreatedAt = existing.CreatedAt == default ? DateTime.Now : existing.CreatedAt;
                    entry.UpdatedAt = DateTime.Now;

                    var updateResult = await _database.UpdateAsync(entry);
                    Console.WriteLine($"Existing entry found for date {entry.Date.Date}. Updated ID: {entry.Id}, Result: {updateResult}");
                    return updateResult;
                }

                // No existing entry for that date - insert
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                var result = await _database.InsertAsync(entry);
                Console.WriteLine($"Entry inserted. ID: {entry.Id}, Result: {result}");
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveEntryAsync error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<int> DeleteEntryAsync(JournalEntry entry)
    {
        try
        {
            return await _database.DeleteAsync(entry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteEntryAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<JournalEntry>> SearchEntriesAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetEntriesAsync();
            }

            query = query.ToLower();
            var entries = await _database.Table<JournalEntry>().ToListAsync();

            return entries
                .Where(e =>
                    e.Title.ToLower().Contains(query) ||
                    e.Content.ToLower().Contains(query) ||
                    e.Tags.ToLower().Contains(query) ||
                    e.PrimaryMood.ToLower().Contains(query) ||
                    e.SecondaryMoods.ToLower().Contains(query))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SearchEntriesAsync error: {ex.Message}");
            return new List<JournalEntry>();
        }
    }

    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            var startDate = start.Date;
            var endDate = end.Date.AddDays(1).AddTicks(-1);

            return await _database.Table<JournalEntry>()
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetEntriesByDateRangeAsync error: {ex.Message}");
            return new List<JournalEntry>();
        }
    }
}