using TodoApp.Client.Gamification;
using TodoApp.Client.Models;
using TodoApp.Client.Services;

namespace TodoApp.Tests;

public class TodoStoreTests
{
    private static TodoStore CreateStore(out AuditLogService audit, out GamificationService gamification)
    {
        gamification = new GamificationService();
        audit = new AuditLogService();
        return new TodoStore(gamification, audit);
    }

    [Fact]
    public void Add_CreatesItemAndLogsAudit()
    {
        var store = CreateStore(out var audit, out _);

        var item = store.Add("Deploy build", Priority.High);

        Assert.Single(store.Items);
        Assert.Equal("Deploy build", item.Text);
        Assert.Contains("Added objective \"Deploy build\"", audit.Entries[0].Message);
    }

    [Fact]
    public void ToggleDone_OnCompletion_AwardsXpAndRecordsPoints()
    {
        var store = CreateStore(out var audit, out var gamification);
        var item = store.Add("Ship it", Priority.Medium);

        store.ToggleDone(item);

        Assert.True(item.Done);
        Assert.Equal(10, item.PointsEarned);
        Assert.Equal(10, gamification.TotalXp);
        Assert.NotNull(item.CompletedAt);
        Assert.Contains("Completed objective", audit.Entries[0].Message);
    }

    [Fact]
    public void ToggleDone_Twice_RevertsXpAndClearsCompletedAt()
    {
        var store = CreateStore(out var audit, out var gamification);
        var item = store.Add("Ship it", Priority.Medium);

        store.ToggleDone(item); // complete
        store.ToggleDone(item); // reopen

        Assert.False(item.Done);
        Assert.Equal(0, item.PointsEarned);
        Assert.Equal(0, gamification.TotalXp);
        Assert.Null(item.CompletedAt);
        Assert.Contains("Reopened objective", audit.Entries[0].Message);
    }

    [Fact]
    public void Delete_RemovesItemAndLogsAudit()
    {
        var store = CreateStore(out var audit, out _);
        var item = store.Add("Temp task", Priority.Low);

        store.Delete(item);

        Assert.Empty(store.Items);
        Assert.Contains("Deleted objective \"Temp task\"", audit.Entries[0].Message);
    }

    [Fact]
    public void Edit_ChangesTextAndLogsOldAndNewText()
    {
        var store = CreateStore(out var audit, out _);
        var item = store.Add("Old text", Priority.Low);

        store.Edit(item, "New text");

        Assert.Equal("New text", item.Text);
        Assert.Contains("\"Old text\" to \"New text\"", audit.Entries[0].Message);
    }

    [Fact]
    public void CompletedTodayCount_OnlyCountsItemsCompletedToday()
    {
        var store = CreateStore(out _, out _);
        var item1 = store.Add("Task 1", Priority.Low);
        var item2 = store.Add("Task 2", Priority.Low);
        store.ToggleDone(item1);
        store.ToggleDone(item2);

        Assert.Equal(2, store.CompletedTodayCount);
    }

    [Fact]
    public void OnCompleted_FiresWithCompletionResult()
    {
        var store = CreateStore(out _, out _);
        var item = store.Add("Task", Priority.High);
        CompletionResult? captured = null;
        store.OnCompleted += (_, result) => captured = result;

        store.ToggleDone(item);

        Assert.NotNull(captured);
        Assert.Equal(20, captured!.PointsEarned);
    }
}
