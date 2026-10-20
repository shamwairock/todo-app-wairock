using TodoApp.Client.Gamification;

namespace TodoApp.Tests;

public class ComboTrackerTests
{
    [Fact]
    public void Register_FirstCall_StartsComboAtOne()
    {
        var tracker = new ComboTracker();
        var combo = tracker.Register(DateTime.Now);

        Assert.Equal(1, combo);
    }

    [Fact]
    public void Register_WithinWindow_IncrementsCombo()
    {
        var tracker = new ComboTracker();
        var t0 = DateTime.Now;

        tracker.Register(t0);
        var combo = tracker.Register(t0 + TimeSpan.FromSeconds(30));

        Assert.Equal(2, combo);
    }

    [Fact]
    public void Register_AtExactWindowBoundary_StillCounts()
    {
        var tracker = new ComboTracker();
        var t0 = DateTime.Now;

        tracker.Register(t0);
        var combo = tracker.Register(t0 + ComboTracker.Window);

        Assert.Equal(2, combo);
    }

    [Fact]
    public void Register_OutsideWindow_ResetsComboToOne()
    {
        var tracker = new ComboTracker();
        var t0 = DateTime.Now;

        tracker.Register(t0);
        tracker.Register(t0 + TimeSpan.FromSeconds(10));
        var combo = tracker.Register(t0 + TimeSpan.FromSeconds(10) + ComboTracker.Window + TimeSpan.FromSeconds(1));

        Assert.Equal(1, combo);
    }

    [Theory]
    [InlineData(1, 1.0)]
    [InlineData(2, 1.1)]
    [InlineData(5, 1.4)]
    [InlineData(11, 2.0)]
    [InlineData(20, 2.0)]
    public void MultiplierFor_ScalesWithComboAndCapsAtMax(int combo, double expectedMultiplier)
    {
        Assert.Equal(expectedMultiplier, ComboTracker.MultiplierFor(combo), precision: 3);
    }

    [Fact]
    public void Reset_ClearsComboAndLastCompletion()
    {
        var tracker = new ComboTracker();
        var t0 = DateTime.Now;

        tracker.Register(t0);
        tracker.Reset();
        var combo = tracker.Register(t0 + TimeSpan.FromSeconds(1));

        Assert.Equal(1, combo);
    }
}
