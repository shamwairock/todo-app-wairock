using TodoApp.Client.Gamification;
using TodoApp.Client.Models;

namespace TodoApp.Tests;

public class GamificationServiceTests
{
    [Fact]
    public void RegisterCompletion_FirstCompletion_AwardsBasePoints()
    {
        var service = new GamificationService();
        var result = service.RegisterCompletion(Priority.Medium, DateTime.Now);

        Assert.Equal(10, result.PointsEarned);
        Assert.Equal(1, result.Combo);
        Assert.Equal(10, result.TotalXp);
        Assert.Equal(10, service.TotalXp);
    }

    [Fact]
    public void RegisterCompletion_WithinComboWindow_AppliesMultiplier()
    {
        var service = new GamificationService();
        var t0 = DateTime.Now;

        service.RegisterCompletion(Priority.Medium, t0);
        var second = service.RegisterCompletion(Priority.Medium, t0 + TimeSpan.FromSeconds(5));

        // combo 2 -> multiplier 1.1 -> 10 * 1.1 = 11
        Assert.Equal(11, second.PointsEarned);
        Assert.Equal(2, second.Combo);
        Assert.Equal(21, service.TotalXp);
    }

    [Fact]
    public void RegisterCompletion_CrossingThreshold_ReportsLeveledUp()
    {
        var service = new GamificationService();

        var result = service.RegisterCompletion(Priority.High, DateTime.Now); // 20 XP, still level 1

        Assert.False(result.LeveledUp);
        Assert.Equal(1, result.Level);

        // Need to reach 100 XP total to hit level 2; space completions far apart so no combo bonus skews the math.
        var t = DateTime.Now;
        for (var i = 0; i < 3; i++)
        {
            t += TimeSpan.FromHours(1);
            result = service.RegisterCompletion(Priority.High, t); // +20 XP each, combo resets (gap > window)
        }

        // total so far: 20 + 20*3 = 80, still level 1
        Assert.False(result.LeveledUp);

        t += TimeSpan.FromHours(1);
        result = service.RegisterCompletion(Priority.High, t); // total 100 -> level 2

        Assert.True(result.LeveledUp);
        Assert.Equal(2, result.Level);
        Assert.Equal(100, service.TotalXp);
    }

    [Fact]
    public void RevertCompletion_SubtractsPointsAndNeverGoesNegative()
    {
        var service = new GamificationService();
        service.RegisterCompletion(Priority.Low, DateTime.Now); // +5

        service.RevertCompletion(5);
        Assert.Equal(0, service.TotalXp);

        service.RevertCompletion(100);
        Assert.Equal(0, service.TotalXp);
    }

    [Fact]
    public void OnChange_FiresOnCompletionAndRevert()
    {
        var service = new GamificationService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        service.RegisterCompletion(Priority.Low, DateTime.Now);
        service.RevertCompletion(5);

        Assert.Equal(2, changeCount);
    }
}
