using JournalApp.Data;
using JournalApp.Models;

namespace JournalApp.Services
{
    public interface IJournalService
    {
        Task<List<JournalEntry>> GetAllEntriesAsync();
        Task<JournalEntry?> GetEntryByDateAsync(DateTime date);
        Task<JournalEntry?> GetEntryByIdAsync(int id);
        Task<bool> SaveEntryAsync(JournalEntry entry);
        Task<bool> DeleteEntryAsync(int id);
        Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm);
        Task<List<JournalEntry>> FilterEntriesAsync(DateTime? startDate, DateTime? endDate, MoodType? mood, List<string>? tags);
        Task<List<JournalEntry>> GetPaginatedEntriesAsync(int page, int pageSize);
        Task<int> GetTotalEntriesCountAsync();
        Task<StreakData> GetStreakDataAsync();
        Task<List<DateTime>> GetEntryDatesAsync(int year, int month);
    }

    public class JournalService : IJournalService
    {
        private readonly JournalDbContext _context;
        private readonly IAuthenticationService _authService;

        public JournalService(JournalDbContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.Id ?? 0;
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            return await _context.GetEntriesAsync(userId);
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            var userId = await GetCurrentUserIdAsync();
            return await _context.GetEntryByDateAsync(userId, date.Date);
        }

        public Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            return _context.GetEntryByIdAsync(id);
        }

        public async Task<bool> SaveEntryAsync(JournalEntry entry)
        {
            var userId = await GetCurrentUserIdAsync();
            entry.UserId = userId;
            entry.EntryDate = entry.EntryDate.Date;

            if (entry.Id == 0)
            {
                var existing = await _context.GetEntryByDateAsync(userId, entry.EntryDate);
                if (existing != null)
                    return false;
            }

            await UpdateTagUsageAsync(entry.TagList);
            await _context.SaveEntryAsync(entry);
            return true;
        }

        public async Task<bool> DeleteEntryAsync(int id)
        {
            var entry = await _context.GetEntryByIdAsync(id);
            if (entry == null)
                return false;

            await _context.DeleteEntryAsync(entry);
            return true;
        }

        public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
        {
            var entries = await GetAllEntriesAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return entries;

            return entries.Where(e =>
                e.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                e.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<List<JournalEntry>> FilterEntriesAsync(DateTime? startDate, DateTime? endDate, MoodType? mood, List<string>? tags)
        {
            var entries = await GetAllEntriesAsync();

            if (startDate.HasValue)
                entries = entries.Where(e => e.EntryDate >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                entries = entries.Where(e => e.EntryDate <= endDate.Value.Date).ToList();

            if (mood.HasValue)
            {
                entries = entries.Where(e =>
                    e.PrimaryMood == mood.Value ||
                    e.SecondaryMood1 == mood.Value ||
                    e.SecondaryMood2 == mood.Value).ToList();
            }

            if (tags != null && tags.Any())
            {
                entries = entries.Where(e =>
                    e.TagList.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                    .ToList();
            }

            return entries;
        }

        public async Task<List<JournalEntry>> GetPaginatedEntriesAsync(int page, int pageSize)
        {
            var entries = await GetAllEntriesAsync();
            return entries
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<int> GetTotalEntriesCountAsync()
        {
            var entries = await GetAllEntriesAsync();
            return entries.Count;
        }

        public async Task<StreakData> GetStreakDataAsync()
        {
            var entries = await GetAllEntriesAsync();
            if (!entries.Any())
            {
                return new StreakData
                {
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    MissedDays = 0,
                    TotalEntries = 0
                };
            }

            var sortedDates = entries
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int currentStreak = 0;
            int longestStreak = 0;
            int tempStreak = 1;
            int missedDays = 0;

            var today = DateTime.Today;
            var lastEntryDate = sortedDates.First();

            if (lastEntryDate == today || lastEntryDate == today.AddDays(-1))
            {
                currentStreak = 1;
                for (int i = 0; i < sortedDates.Count - 1; i++)
                {
                    var diff = (sortedDates[i] - sortedDates[i + 1]).Days;
                    if (diff == 1)
                        currentStreak++;
                    else
                        break;
                }
            }

            for (int i = 0; i < sortedDates.Count - 1; i++)
            {
                var diff = (sortedDates[i] - sortedDates[i + 1]).Days;
                if (diff == 1)
                {
                    tempStreak++;
                }
                else
                {
                    if (tempStreak > longestStreak)
                        longestStreak = tempStreak;
                    tempStreak = 1;
                    missedDays += diff - 1;
                }
            }

            if (tempStreak > longestStreak)
                longestStreak = tempStreak;

            return new StreakData
            {
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                MissedDays = missedDays,
                TotalEntries = entries.Count
            };
        }

        public async Task<List<DateTime>> GetEntryDatesAsync(int year, int month)
        {
            var entries = await GetAllEntriesAsync();
            return entries
                .Where(e => e.EntryDate.Year == year && e.EntryDate.Month == month)
                .Select(e => e.EntryDate.Date)
                .ToList();
        }

        private async Task UpdateTagUsageAsync(List<string> tags)
        {
            var userId = await GetCurrentUserIdAsync();
            foreach (var tagName in tags)
            {
                var tag = await _context.GetTagByNameAsync(userId, tagName);
                if (tag == null)
                {
                    tag = new Tag
                    {
                        UserId = userId,
                        Name = tagName,
                        UsageCount = 1
                    };
                }
                else
                {
                    tag.UsageCount++;
                }
                await _context.SaveTagAsync(tag);
            }
        }
    }
}