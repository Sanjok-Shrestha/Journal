using JournalApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JournalApp.Services
{
    public interface IPdfExportService
    {
        Task<string> ExportToPdfAsync(List<JournalEntry> entries, DateTime startDate, DateTime endDate);
    }

    public class PdfExportService : IPdfExportService
    {
        public async Task<string> ExportToPdfAsync(List<JournalEntry> entries, DateTime startDate, DateTime endDate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var fileName = $"Journal_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text($"Journal Entries: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            foreach (var entry in entries)
                            {
                                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10);

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text(entry.Title).FontSize(16).SemiBold();
                                        col.Item().Text($"Date: {entry.EntryDate:MMM dd, yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                                        col.Item().Text($"Mood: {entry.PrimaryMood.GetEmoji()} {entry.PrimaryMood}").FontSize(10);

                                        if (entry.TagList.Any())
                                        {
                                            col.Item().Text($"Tags: {string.Join(", ", entry.TagList)}").FontSize(10).FontColor(Colors.Blue.Medium);
                                        }
                                    });
                                });

                                var plainContent = System.Text.RegularExpressions.Regex.Replace(entry.Content, "<.*?>", string.Empty);
                                column.Item().PaddingTop(10).Text(plainContent).FontSize(11).LineHeight(1.5f);
                                column.Item().Text($"Words: {entry.WordCount}").FontSize(9).FontColor(Colors.Grey.Medium).AlignRight();
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            })
            .GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }
    }
}