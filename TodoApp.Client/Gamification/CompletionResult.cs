namespace TodoApp.Client.Gamification;

public record CompletionResult(int PointsEarned, int Combo, int TotalXp, int Level, bool LeveledUp);
