namespace JournalApp.Services;

public class ThemeService
{
    private string currentTheme = "light";
    public event Action? OnThemeChanged;

    public string CurrentTheme => currentTheme;

    public void SetTheme(string theme)
    {
        currentTheme = theme;
        OnThemeChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        currentTheme = currentTheme == "light" ? "dark" : "light";
        OnThemeChanged?.Invoke();
    }

    public string GetThemeClass()
    {
        return currentTheme == "dark" ? "dark-theme" : "light-theme";
    }
}