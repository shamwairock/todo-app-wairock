namespace TodoApp.Client.Gamification;

/// <summary>
/// Tracks a completion "combo": consecutive completions within <see cref="Window"/> of each
/// other keep increasing the combo count; a gap longer than the window resets it to 1.
/// </summary>
public class ComboTracker
{
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(2);
    private const double MaxMultiplier = 2.0;
    private const double StepPerCombo = 0.1;

    public int Combo { get; private set; }
    private DateTime? _lastCompletionAt;

    public int Register(DateTime now)
    {
        if (_lastCompletionAt is { } last && now - last <= Window)
        {
            Combo++;
        }
        else
        {
            Combo = 1;
        }
        _lastCompletionAt = now;
        return Combo;
    }

    public static double MultiplierFor(int combo)
    {
        var multiplier = 1.0 + StepPerCombo * (combo - 1);
        return Math.Min(multiplier, MaxMultiplier);
    }

    public void Reset()
    {
        Combo = 0;
        _lastCompletionAt = null;
    }
}
