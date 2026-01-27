using SQLite;
using JournalApp.Models;
using System.Linq;

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

    // Alias used by some components
    public Task<JournalEntry?> GetEntryByIdAsync(int id) => GetEntryAsync(id);

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

    // 🟢 ADD THIS METHOD (for a single entry by date)
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

    public async Task<List<JournalEntry>> GetEntriesByDateAsync(DateTime date)
    {
        try
        {
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

    public async Task<int> SaveEntryAsync(JournalEntry entry)
    {
        try
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            entry.WordCount = string.IsNullOrWhiteSpace(entry.Content)
                ? 0
                : entry.Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (entry.Id != 0)
            {
                entry.UpdatedAt = DateTime.Now;
                var result = await _database.UpdateAsync(entry);
                Console.WriteLine($"Entry updated. ID: {entry.Id}, Result: {result}");
                return result;
            }
            else
            {
                var existing = await GetEntryByDateAsync(entry.Date == default ? DateTime.Today : entry.Date); // Uses the new method
                if (existing != null)
                {
                    entry.Id = existing.Id;
                    entry.CreatedAt = existing.CreatedAt == default ? DateTime.Now : existing.CreatedAt;
                    entry.UpdatedAt = DateTime.Now;

                    var updateResult = await _database.UpdateAsync(entry);
                    Console.WriteLine($"Existing entry found for date {entry.Date.Date}. Updated ID: {entry.Id}, Result: {updateResult}");
                    return updateResult;
                }

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

    // Added: compute streak data for Home.razor
    public async Task<StreakData> GetStreakDataAsync()
    {
        var entries = await GetEntriesAsync();
        var dates = entries.Select(e => e.Date.Date).Distinct().OrderByDescending(d => d).ToList();

        var result = new StreakData();
        if (!dates.Any()) return result;

        // Current streak
        var today = DateTime.Today;
        int current = 0;
        var check = today;
        while (dates.Contains(check))
        {
            current++;
            check = check.AddDays(-1);
        }

        // Longest streak
        int longest = 0;
        int temp = 1;
        var sortedAsc = dates.OrderBy(d => d).ToList();
        for (int i = 1; i < sortedAsc.Count; i++)
        {
            if ((sortedAsc[i] - sortedAsc[i - 1]).Days == 1)
            {
                temp++;
            }
            else
            {
                longest = Math.Max(longest, temp);
                temp = 1;
            }
        }
        longest = Math.Max(longest, temp);

        result.CurrentStreak = current;
        result.LongestStreak = longest;
        result.TotalEntries = entries.Count;
        result.MissedDays = Math.Max(0, (int)(DateTime.Today - dates.Min()).TotalDays + 1 - entries.Count);

        return result;
    }
}