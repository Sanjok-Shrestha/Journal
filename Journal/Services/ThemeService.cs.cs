namespace JournalApp.Services
{
    public class ThemeService
    {
        private string _currentTheme = "Light";

        public event Action? OnThemeChanged;

        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    Preferences.Set("app_theme", value);
                    OnThemeChanged?.Invoke();
                }
            }
        }

        public ThemeService()
        {
            _currentTheme = Preferences.Get("app_theme", "Light");
        }

        public void SetTheme(string theme)
        {
            CurrentTheme = theme;
        }

        public Dictionary<string, string> GetThemeColors()
        {
            return CurrentTheme switch
            {
                "Dark" => new Dictionary<string, string>
                {
                    ["Background"] = "#1a1a1a",
                    ["Surface"] = "#2d2d2d",
                    ["Primary"] = "#bb86fc",
                    ["Text"] = "#ffffff",
                    ["TextSecondary"] = "#b3b3b3",
                    ["Border"] = "#404040"
                },
                "Light" => new Dictionary<string, string>
                {
                    ["Background"] = "#ffffff",
                    ["Surface"] = "#f5f5f5",
                    ["Primary"] = "#6200ee",
                    ["Text"] = "#000000",
                    ["TextSecondary"] = "#666666",
                    ["Border"] = "#e0e0e0"
                },
                _ => new Dictionary<string, string>
                {
                    ["Background"] = "#ffffff",
                    ["Surface"] = "#f5f5f5",
                    ["Primary"] = "#6200ee",
                    ["Text"] = "#000000",
                    ["TextSecondary"] = "#666666",
                    ["Border"] = "#e0e0e0"
                }
            };
        }
    }
}