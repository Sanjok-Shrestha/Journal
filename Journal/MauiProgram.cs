using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using JournalApp.Services;
using Microsoft.Maui.Storage;

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

            // Database path
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            Console.WriteLine($"Database path: {dbPath}");
            Console.WriteLine($"AppData directory: {FileSystem.AppDataDirectory}");

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

            // Register Auth and Theme services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ThemeService>();

            return builder.Build();
        }
    }
}