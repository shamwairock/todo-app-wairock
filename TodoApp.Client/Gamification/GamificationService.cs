using TodoApp.Client.Models;

namespace TodoApp.Client.Gamification;

/// <summary>
/// Owns the player's XP/level/combo state. Pure, UI-agnostic logic lives in
/// <see cref="LevelCalculator"/> and <see cref="ComboTracker"/> so it can be unit tested
/// without any Blazor dependency; this class wires that logic to app state and change
/// notifications for the UI.
/// </summary>
public class GamificationService
{
    private readonly ComboTracker _combo = new();

    public int TotalXp { get; private set; }
    public int Level => LevelCalculator.GetLevel(TotalXp);
    public int Combo => _combo.Combo;

    public event Action? OnChange;

    public CompletionResult RegisterCompletion(Priority priority, DateTime now)
    {
        var levelBefore = Level;
        var combo = _combo.Register(now);
        var basePoints = PriorityPoints.BasePoints(priority);
        var points = (int)Math.Round(basePoints * ComboTracker.MultiplierFor(combo));

        TotalXp += points;
        var levelAfter = Level;

        OnChange?.Invoke();
        return new CompletionResult(points, combo, TotalXp, levelAfter, levelAfter > levelBefore);
    }

    public void RevertCompletion(int points)
    {
        TotalXp = Math.Max(0, TotalXp - points);
        OnChange?.Invoke();
    }

    public void LoadState(int totalXp)
    {
        TotalXp = Math.Max(0, totalXp);
        OnChange?.Invoke();
    }
}
