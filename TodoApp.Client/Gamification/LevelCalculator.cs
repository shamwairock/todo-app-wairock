namespace TodoApp.Client.Gamification;

/// <summary>
/// Cumulative XP required to REACH a level follows Threshold(L) = 50 * (L*(L+1)/2 - 1),
/// giving level-up increments of 100, 150, 200, 250... XP (Threshold(1) = 0).
/// </summary>
public static class LevelCalculator
{
    public static int ThresholdForLevel(int level)
    {
        if (level < 1) level = 1;
        return 50 * (level * (level + 1) / 2 - 1);
    }

    public static int GetLevel(int totalXp)
    {
        var level = 1;
        while (ThresholdForLevel(level + 1) <= totalXp)
        {
            level++;
        }
        return level;
    }

    public static (int Level, int XpIntoLevel, int XpForNextLevel) GetProgress(int totalXp)
    {
        var level = GetLevel(totalXp);
        var currentThreshold = ThresholdForLevel(level);
        var nextThreshold = ThresholdForLevel(level + 1);
        return (level, totalXp - currentThreshold, nextThreshold - currentThreshold);
    }
}
