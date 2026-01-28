using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JournalApp.Models;
using SQLite;

namespace JournalApp.Services;

public class TagService
{
    private readonly SQLiteAsyncConnection _db;

    public TagService(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentException("Database path must be provided.", nameof(dbPath));

        try
        {
            _db = new SQLiteAsyncConnection(dbPath);
            // Ensure tables exist (synchronously during construction)
            _db.CreateTableAsync<Tag>().GetAwaiter().GetResult();
            _db.CreateTableAsync<JournalEntryTag>().GetAwaiter().GetResult();
            Debug.WriteLine("Tag tables initialized");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TagService initialization error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        try
        {
            var tags = await _db.Table<Tag>()
                .OrderBy(t => t.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            if (tags == null || !tags.Any())
            {
                tags = StaticData.GetAllTags();
                try
                {
                    await _db.InsertAllAsync(tags).ConfigureAwait(false);
                }
                catch (Exception insertEx)
                {
                    Debug.WriteLine($"InsertAllAsync fallback failed: {insertEx.Message}");
                }
            }

            return tags;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllTagsAsync error: {ex.Message}");
            return StaticData.GetAllTags();
        }
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        try
        {
            return await _db.Table<Tag>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetTagByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Tag>> GetTagsByIdsAsync(IEnumerable<int> tagIds)
    {
        try
        {
            if (tagIds == null) return new List<Tag>();
            var ids = tagIds as int[] ?? tagIds.ToArray();
            if (!ids.Any()) return new List<Tag>();

            return await _db.Table<Tag>()
                .Where(t => ids.Contains(t.Id))
                .OrderBy(t => t.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetTagsByIdsAsync error: {ex.Message}");
            return new List<Tag>();
        }
    }

    public async Task<List<int>> GetTagIdsForEntryAsync(int journalEntryId)
    {
        try
        {
            var tags = await _db.Table<JournalEntryTag>()
                .Where(jet => jet.JournalEntryId == journalEntryId)
                .ToListAsync()
                .ConfigureAwait(false);
            return tags.Select(jet => jet.TagId).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetTagIdsForEntryAsync error: {ex.Message}");
            return new List<int>();
        }
    }

    public async Task<int> SaveTagAsync(Tag tag)
    {
        if (tag == null) return 0;

        try
        {
            if (tag.Id != 0)
            {
                await _db.UpdateAsync(tag).ConfigureAwait(false);
                return tag.Id;
            }
            else
            {
                await _db.InsertAsync(tag).ConfigureAwait(false);
                return tag.Id;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveTagAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteTagAsync(int id)
    {
        try
        {
            var tag = await GetTagByIdAsync(id).ConfigureAwait(false);
            if (tag is null) return 0;
            return await _db.DeleteAsync(tag).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteTagAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task PrepopulateTagsAsync()
    {
        try
        {
            var existingCount = await _db.Table<Tag>().CountAsync().ConfigureAwait(false);
            if (existingCount > 0)
            {
                Debug.WriteLine("Tags already prepopulated");
                return;
            }

            var tags = StaticData.GetAllTags();
            await _db.InsertAllAsync(tags).ConfigureAwait(false);
            Debug.WriteLine($"Prepopulated {tags.Count} tags");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PrepopulateTagsAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task SaveTagsForEntryAsync(int journalEntryId, List<int> tagIds)
    {
        try
        {
            // Remove existing tags for this entry
            await _db.Table<JournalEntryTag>()
                .DeleteAsync(jet => jet.JournalEntryId == journalEntryId)
                .ConfigureAwait(false);

            // Add new tags (if any)
            if (tagIds == null || !tagIds.Any())
            {
                Debug.WriteLine($"Cleared tags for journal entry {journalEntryId}");
                return;
            }

            var entryTags = tagIds.Select(tagId => new JournalEntryTag
            {
                JournalEntryId = journalEntryId,
                TagId = tagId
            }).ToList();

            await _db.InsertAllAsync(entryTags).ConfigureAwait(false);
            Debug.WriteLine($"Saved {tagIds.Count} tags for journal entry {journalEntryId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveTagsForEntryAsync error: {ex.Message}");
            throw;
        }
    }
}
