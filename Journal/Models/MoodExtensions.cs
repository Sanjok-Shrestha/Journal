namespace JournalApp.Models;

public static class MoodExtensions
{
    public static string GetEmoji(this string mood)
    {
        if (string.IsNullOrWhiteSpace(mood)) return "??";
        return mood switch
        {
            "Happy" => "??",
            "Sad" => "??",
            "Calm" => "??",
            "Excited" => "??",
            "Anxious" => "??",
            "Grateful" => "??",
            "Tired" => "??",
            "Angry" => "??",
            "Stressed" => "??",
            "Confident" => "??",
            "Relaxed" => "??",
            "Thoughtful" => "??",
            "Curious" => "??",
            "Nostalgic" => "???",
            "Bored" => "??",
            "Lonely" => "??",
            _ => "??"
        };
    }

    public static string GetColor(this string mood)
    {
        if (string.IsNullOrWhiteSpace(mood)) return "#999";
        return mood switch
        {
            "Happy" => "#FFD54F",
            "Sad" => "#90CAF9",
            "Calm" => "#A5D6A7",
            "Excited" => "#FF8A65",
            "Anxious" => "#F06292",
            "Grateful" => "#CE93D8",
            "Tired" => "#B0BEC5",
            "Angry" => "#E57373",
            "Stressed" => "#EF9A9A",
            "Confident" => "#81C784",
            "Relaxed" => "#4DB6AC",
            _ => "#999"
        };
    }

    public static string GetEmoji(this MoodType mood)
        => mood.ToString().GetEmoji();

    public static string GetColor(this MoodType mood)
        => mood.ToString().GetColor();
}