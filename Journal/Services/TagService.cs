using SQLite;
using JournalApp.Models;
namespace JournalApp.Models;

public class TagService
{
    private readonly SQLiteAsyncConnection _database;

    public TagService(string dbPath)
    {
        try
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Tag>().Wait();
            _database.CreateTableAsync<JournalEntryTag>().Wait();
            Console.WriteLine("Tag tables initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TagService initialization error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        try
        {
            var tags = await _database.Table<Tag>()
                .OrderBy(t => t.Name)
                .ToListAsync();

            if (tags == null || !tags.Any())
            {
                tags = StaticData.GetAllTags();
                try { await _database.InsertAllAsync(tags); } catch { }
            }

            return tags;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllTagsAsync error: {ex.Message}");
            return StaticData.GetAllTags();
        }
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        try
        {
            return await _database.Table<Tag>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetTagByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Tag>> GetTagsByIdsAsync(IEnumerable<int> tagIds)
    {
        try
        {
            return await _database.Table<Tag>()
                .Where(t => tagIds.Contains(t.Id))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetTagsByIdsAsync error: {ex.Message}");
            return new List<Tag>();
        }
    }

    public async Task<List<int>> GetTagIdsForEntryAsync(int journalEntryId)
    {
        try
        {
            var tags = await _database.Table<JournalEntryTag>()
                .Where(jet => jet.JournalEntryId == journalEntryId)
                .ToListAsync();
            return tags.Select(jet => jet.TagId).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetTagIdsForEntryAsync error: {ex.Message}");
            return new List<int>();
        }
    }

    public async Task<int> SaveTagAsync(Tag tag)
    {
        try
        {
            if (tag.Id != 0)
            {
                await _database.UpdateAsync(tag);
                return tag.Id;
            }
            else
            {
                await _database.InsertAsync(tag);
                return tag.Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveTagAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteTagAsync(int id)
    {
        try
        {
            var tag = await GetTagByIdAsync(id);
            if (tag == null) return 0;
            return await _database.DeleteAsync(tag);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteTagAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task PrepopulateTagsAsync()
    {
        try
        {
            var existingCount = await _database.Table<Tag>().CountAsync();
            if (existingCount > 0)
            {
                Console.WriteLine("Tags already prepopulated");
                return;
            }

            var tags = StaticData.GetAllTags();

            await _database.InsertAllAsync(tags);
            Console.WriteLine($"Prepopulated {tags.Count} tags");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PrepopulateTagsAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task SaveTagsForEntryAsync(int journalEntryId, List<int> tagIds)
    {
        try
        {
            // Remove existing tags for this entry
            await _database.Table<JournalEntryTag>()
                .DeleteAsync(jet => jet.JournalEntryId == journalEntryId);

            // Add new tags
            if (tagIds.Any())
            {
                var entryTags = tagIds.Select(tagId => new JournalEntryTag
                {
                    JournalEntryId = journalEntryId,
                    TagId = tagId
                }).ToList();

                await _database.InsertAllAsync(entryTags);
            }

            Console.WriteLine($"Saved {tagIds.Count} tags for journal entry {journalEntryId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveTagsForEntryAsync error: {ex.Message}");
            throw;
        }
    }
}