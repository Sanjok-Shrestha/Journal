using Microsoft.Extensions.Logging;
using JournalApp.Data;
using JournalApp.Services;

namespace JournalApp;

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

        // Register SQLite Database Context
        builder.Services.AddSingleton<JournalDbContext>();

        // Register Services
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IJournalService, JournalService>();
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
        builder.Services.AddSingleton<ThemeService>();

        return builder.Build();
    }
}