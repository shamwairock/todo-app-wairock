using TodoApp.Client.Gamification;

namespace TodoApp.Tests;

public class LevelCalculatorTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(249, 2)]
    [InlineData(250, 3)]
    [InlineData(449, 3)]
    [InlineData(450, 4)]
    [InlineData(699, 4)]
    [InlineData(700, 5)]
    [InlineData(999, 5)]
    [InlineData(1000, 6)]
    public void GetLevel_ReturnsExpectedLevelForXp(int xp, int expectedLevel)
    {
        Assert.Equal(expectedLevel, LevelCalculator.GetLevel(xp));
    }

    [Fact]
    public void ThresholdForLevel_IsZeroAtLevelOne()
    {
        Assert.Equal(0, LevelCalculator.ThresholdForLevel(1));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 250)]
    [InlineData(4, 450)]
    [InlineData(5, 700)]
    public void ThresholdForLevel_MatchesExpectedSequence(int level, int expectedThreshold)
    {
        Assert.Equal(expectedThreshold, LevelCalculator.ThresholdForLevel(level));
    }

    [Fact]
    public void GetProgress_ReportsXpIntoLevelAndXpNeededForNext()
    {
        var (level, xpIntoLevel, xpForNextLevel) = LevelCalculator.GetProgress(120);

        Assert.Equal(2, level);
        Assert.Equal(20, xpIntoLevel);
        Assert.Equal(150, xpForNextLevel);
    }

    [Fact]
    public void GetProgress_AtExactThreshold_StartsFreshInNewLevel()
    {
        var (level, xpIntoLevel, _) = LevelCalculator.GetProgress(250);

        Assert.Equal(3, level);
        Assert.Equal(0, xpIntoLevel);
    }
}
