using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using JournalApp.Models;

namespace JournalApp.Services;

/// <summary>
/// Service responsible for CRUD and lookup operations for <see cref="Mood"/> records.
/// Uses SQLite-net's <see cref="SQLiteAsyncConnection"/> to persist data locally.
/// </summary>
public class MoodService
{
    private readonly SQLiteAsyncConnection _database;

    // Standardized category values used by the app.
    private const string PrimaryCategory = "Primary";
    private const string SecondaryCategory = "Secondary";

    public MoodService(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentException("Database path must be provided", nameof(dbPath));

        try
        {
            _database = new SQLiteAsyncConnection(dbPath);

            // Ensure the Mood table exists. Run synchronously from ctor using GetAwaiter().GetResult
            // because constructors cannot be async. This avoids wrapping AggregateException from .Wait().
            _database.CreateTableAsync<Mood>().GetAwaiter().GetResult();
            Debug.WriteLine("Mood table initialized");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MoodService initialization error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Returns all moods from the database ordered by Category then Name.
    /// Falls back to StaticData if the DB is empty or on error.
    /// </summary>
    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        try
        {
            var moods = await _database.Table<Mood>()
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            if (moods == null || !moods.Any())
            {
                // Fallback to static data (and attempt a best-effort insert)
                moods = StaticData.GetAllMoods();
                try
                {
                    await _database.InsertAllAsync(moods).ConfigureAwait(false);
                }
                catch (Exception insertEx)
                {
                    Debug.WriteLine($"GetAllMoodsAsync: fallback insert failed: {insertEx.Message}");
                }
            }

            return moods;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllMoodsAsync error: {ex.Message}");
            return StaticData.GetAllMoods();
        }
    }

    /// <summary>
    /// Returns moods filtered by category (e.g. "Primary" or "Secondary").
    /// Falls back to static data if the DB query returns nothing or on error.
    /// </summary>
    public async Task<List<Mood>> GetMoodsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return new List<Mood>();

        try
        {
            var moods = await _database.Table<Mood>()
                .Where(m => m.Category == category)
                .OrderBy(m => m.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            if (moods == null || !moods.Any())
            {
                moods = StaticData.GetAllMoods().Where(m => m.Category == category).ToList();
            }

            return moods;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetMoodsByCategoryAsync error: {ex.Message}");
            return StaticData.GetAllMoods().Where(m => m.Category == category).ToList();
        }
    }

    /// <summary>
    /// Retrieve a mood by its integer id, or null if not found.
    /// </summary>
    public async Task<Mood?> GetMoodByIdAsync(int id)
    {
        try
        {
            return await _database.Table<Mood>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetMoodByIdAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Insert or update a mood. Performs normalization/validation on Category and MoodType,
    /// and attempts to avoid duplicate names by updating an existing record when possible.
    /// Returns the number of rows affected (Update) or the inserted Id (insert path returns mood.Id).
    /// On error returns 0.
    /// </summary>
    public async Task<int> SaveMoodAsync(Mood mood)
    {
        if (mood == null) throw new ArgumentNullException(nameof(mood));

        try
        {
            // Trim/normalize textual fields
            mood.Name = (mood.Name ?? string.Empty).Trim();
            mood.Category = (mood.Category ?? string.Empty).Trim();
            mood.MoodType = (mood.MoodType ?? string.Empty).Trim();

            // Ensure category is either Primary or Secondary (default to Secondary)
            if (!string.Equals(mood.Category, PrimaryCategory, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(mood.Category, SecondaryCategory, StringComparison.OrdinalIgnoreCase))
            {
                mood.Category = SecondaryCategory;
            }
            else
            {
                // Normalize casing
                mood.Category = string.Equals(mood.Category, PrimaryCategory, StringComparison.OrdinalIgnoreCase)
                    ? PrimaryCategory : SecondaryCategory;
            }

            // Normalize MoodType to one of Positive/Neutral/Negative or default to Neutral
            var mt = mood.MoodType;
            if (string.Equals(mt, "Positive", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mt, "Negative", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mt, "Neutral", StringComparison.OrdinalIgnoreCase))
            {
                mood.MoodType = mt.First().ToString().ToUpper() + mt.Substring(1).ToLower();
            }
            else
            {
                mood.MoodType = "Neutral";
            }

            // If a mood with the same name exists, prefer updating that record to avoid duplicates.
            var existingByName = await _database.Table<Mood>()
                .Where(m => m.Name == mood.Name)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (mood.Id != 0)
            {
                // If the caller provided an Id but another record with the same name exists,
                // adjust the target id so we update the existing record instead of creating a duplicate.
                if (existingByName != null && existingByName.Id != mood.Id)
                {
                    mood.Id = existingByName.Id;
                }

                // Update returns the number of rows affected.
                return await _database.UpdateAsync(mood).ConfigureAwait(false);
            }
            else
            {
                if (existingByName != null)
                {
                    // Update the existing record with new category/type values.
                    mood.Id = existingByName.Id;
                    return await _database.UpdateAsync(mood).ConfigureAwait(false);
                }

                // Insert new record and return its Id
                await _database.InsertAsync(mood).ConfigureAwait(false);
                return mood.Id;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveMoodAsync error: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Delete a mood by id. Returns number of rows deleted (0 if not found or on error).
    /// </summary>
    public async Task<int> DeleteMoodAsync(int id)
    {
        try
        {
            var mood = await GetMoodByIdAsync(id).ConfigureAwait(false);
            if (mood == null) return 0;
            return await _database.DeleteAsync(mood).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteMoodAsync error: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Returns the distinct categories present in the moods table (e.g. Primary, Secondary).
    /// Falls back to static data on error.
    /// </summary>
    public async Task<List<string>> GetCategoriesAsync()
    {
        try
        {
            var moods = await GetAllMoodsAsync().ConfigureAwait(false);
            return moods.Select(m => m.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCategoriesAsync error: {ex.Message}");
            return StaticData.GetAllMoods().Select(m => m.Category).Distinct().OrderBy(c => c).ToList();
        }
    }

    /// <summary>
    /// Inserts any missing static moods from StaticData into the database.
    /// This is safe to call multiple times; it only inserts moods with names not already present.
    /// </summary>
    public async Task PrepopulateMoodsAsync()
    {
        try
        {
            var existing = await _database.Table<Mood>().ToListAsync().ConfigureAwait(false);
            var existingNames = existing.Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var staticMoods = StaticData.GetAllMoods();
            var toInsert = staticMoods.Where(s => !existingNames.Contains(s.Name)).ToList();

            if (toInsert.Any())
            {
                await _database.InsertAllAsync(toInsert).ConfigureAwait(false);
                Debug.WriteLine($"Prepopulated {toInsert.Count} missing moods");
            }
            else
            {
                Debug.WriteLine("No missing moods to prepopulate");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PrepopulateMoodsAsync error: {ex.Message}");
            throw;
        }
    }
}
