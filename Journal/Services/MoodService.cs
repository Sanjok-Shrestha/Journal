using SQLite;
using JournalApp.Models;

namespace JournalApp.Services;

public class MoodService
{
    private readonly SQLiteAsyncConnection _database;

    public MoodService(string dbPath)
    {
        try
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Mood>().Wait();
            Console.WriteLine("Mood table initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MoodService initialization error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        try
        {
            var moods = await _database.Table<Mood>()
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            if (moods == null || !moods.Any())
            {
                // fallback to static data
                moods = StaticData.GetAllMoods();
                // try to insert into DB asynchronously
                try { await _database.InsertAllAsync(moods); } catch { }
            }

            return moods;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllMoodsAsync error: {ex.Message}");
            return StaticData.GetAllMoods();
        }
    }

    public async Task<List<Mood>> GetMoodsByCategoryAsync(string category)
    {
        try
        {
            var moods = await _database.Table<Mood>()
                .Where(m => m.Category == category)
                .OrderBy(m => m.Name)
                .ToListAsync();

            if (moods == null || !moods.Any())
            {
                moods = StaticData.GetAllMoods().Where(m => m.Category == category).ToList();
            }

            return moods;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetMoodsByCategoryAsync error: {ex.Message}");
            return StaticData.GetAllMoods().Where(m => m.Category == category).ToList();
        }
    }

    public async Task<Mood?> GetMoodByIdAsync(int id)
    {
        try
        {
            return await _database.Table<Mood>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetMoodByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<int> SaveMoodAsync(Mood mood)
    {
        try
        {
            if (mood == null) throw new ArgumentNullException(nameof(mood));

            // Normalize Category to either "Primary" or "Secondary"
            var cat = (mood.Category ?? string.Empty).Trim();
            if (!string.Equals(cat, "Primary", StringComparison.OrdinalIgnoreCase) && !string.Equals(cat, "Secondary", StringComparison.OrdinalIgnoreCase))
            {
                // default to Secondary if invalid
                mood.Category = "Secondary";
            }
            else
            {
                mood.Category = string.Equals(cat, "Primary", StringComparison.OrdinalIgnoreCase) ? "Primary" : "Secondary";
            }

            // Normalize MoodType to Positive/Neutral/Negative when possible
            var mt = (mood.MoodType ?? string.Empty).Trim();
            if (string.Equals(mt, "Positive", StringComparison.OrdinalIgnoreCase) || string.Equals(mt, "Negative", StringComparison.OrdinalIgnoreCase) || string.Equals(mt, "Neutral", StringComparison.OrdinalIgnoreCase))
            {
                mood.MoodType = mt.First().ToString().ToUpper() + mt.Substring(1).ToLower();
            }
            else if (string.IsNullOrEmpty(mt))
            {
                // default to Neutral
                mood.MoodType = "Neutral";
            }

            // Prevent duplicate names: if a mood with same name exists, update it instead of inserting duplicate
            var existingByName = await _database.Table<Mood>().Where(m => m.Name == mood.Name).FirstOrDefaultAsync();

            if (mood.Id != 0)
            {
                // If the Id is set but name matches another record, use that record's id
                if (existingByName != null && existingByName.Id != mood.Id)
                {
                    mood.Id = existingByName.Id;
                }

                return await _database.UpdateAsync(mood);
            }
            else
            {
                if (existingByName != null)
                {
                    // update existing record
                    mood.Id = existingByName.Id;
                    return await _database.UpdateAsync(mood);
                }

                // insert new
                await _database.InsertAsync(mood);
                return mood.Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveMoodAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteMoodAsync(int id)
    {
        try
        {
            var mood = await GetMoodByIdAsync(id);
            if (mood == null) return 0;
            return await _database.DeleteAsync(mood);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteMoodAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        try
        {
            var moods = await GetAllMoodsAsync();
            return moods.Select(m => m.Category).Distinct().OrderBy(c => c).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetCategoriesAsync error: {ex.Message}");
            return StaticData.GetAllMoods().Select(m => m.Category).Distinct().OrderBy(c => c).ToList();
        }
    }

    public async Task PrepopulateMoodsAsync()
    {
        try
        {
            // Insert only missing static moods to avoid duplicates; this allows repopulation without duplicating
            var existing = await _database.Table<Mood>().ToListAsync();
            var existingNames = existing.Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var staticMoods = StaticData.GetAllMoods();
            var toInsert = staticMoods.Where(s => !existingNames.Contains(s.Name)).ToList();

            if (toInsert.Any())
            {
                await _database.InsertAllAsync(toInsert);
                Console.WriteLine($"Prepopulated {toInsert.Count} missing moods");
            }
            else
            {
                Console.WriteLine("No missing moods to prepopulate");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PrepopulateMoodsAsync error: {ex.Message}");
            throw;
        }
    }
}