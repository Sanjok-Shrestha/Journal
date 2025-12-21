using Journal.Models;

namespace Journal.Services
{
    public class JournalService
    {
        private readonly List<JournalEntry> _entries = new();

        public JournalEntry? GetEntryByDate(DateTime date)
        {
            return _entries.FirstOrDefault(e => e.EntryDate == date.Date);
        }

        public void SaveEntry(JournalEntry entry)
        {
            var existing = GetEntryByDate(entry.EntryDate);

            if (existing == null)
            {
                entry.Id = _entries.Count + 1;
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                _entries.Add(entry);
            }
            else
            {
                existing.Content = entry.Content;
                existing.PrimaryMood = entry.PrimaryMood;
                existing.SecondaryMood1 = entry.SecondaryMood1;
                existing.SecondaryMood2 = entry.SecondaryMood2;
                existing.Tags = entry.Tags;
                existing.UpdatedAt = DateTime.Now;
            }
        }

        public void DeleteEntry(DateTime date)
        {
            var entry = GetEntryByDate(date);
            if (entry != null)
            {
                _entries.Remove(entry);
            }
        }
    }
}
