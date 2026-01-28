using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace JournalApp.Platforms.Windows
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[UNHANDLED EXCEPTION] {e.Exception?.ToString()}"
                );
            };
            InitializeComponent();
        }
       
        protected override MauiApp CreateMauiApp()
        {
            return MauiProgram.CreateMauiApp();
        }
    }
}