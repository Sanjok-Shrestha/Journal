using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using JournalApp.Services;
using Microsoft.Maui.Storage;
using System.IO;

namespace JournalApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            builder.Services.AddMudServices();

            // Database path fix for Windows!
            string dbPath;
#if WINDOWS
            dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "journal.db"
            );
#else
            dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
#endif

            Console.WriteLine($"Database path: {dbPath}");

            // Ensure directory exists
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
                Console.WriteLine($"Created database directory: {dbDirectory}");
            }

            // Register JournalService
            builder.Services.AddSingleton<JournalService>(s =>
            {
                try
                {
                    return new JournalService(dbPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating JournalService: {ex.Message}");
                    throw;
                }
            });

            // Register UserService
            builder.Services.AddSingleton<UserService>(s =>
            {
                try
                {
                    return new UserService(dbPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating UserService: {ex.Message}");
                    throw;
                }
            });

            // Register MoodService (ASYNC prepopulation)
            builder.Services.AddSingleton<MoodService>(s =>
            {
                try
                {
                    var service = new MoodService(dbPath);
                    Task.Run(() => service.PrepopulateMoodsAsync());
                    return service;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating MoodService: {ex.Message}");
                    throw;
                }
            });

            // Register TagService (ASYNC prepopulation)
            builder.Services.AddSingleton<TagService>(s =>
            {
                try
                {
                    var service = new TagService(dbPath);
                    Task.Run(() => service.PrepopulateTagsAsync());
                    return service;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating TagService: {ex.Message}");
                    throw;
                }
            });

            // Register Auth and Theme services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ThemeService>();

            return builder.Build();
        }
    }
}