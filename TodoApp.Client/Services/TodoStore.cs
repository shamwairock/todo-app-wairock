using TodoApp.Client.Gamification;
using TodoApp.Client.Models;

namespace TodoApp.Client.Services;

public class TodoStore
{
    private readonly List<TodoItem> _items = new();
    private readonly GamificationService _gamification;
    private readonly AuditLogService _audit;

    public TodoStore(GamificationService gamification, AuditLogService audit)
    {
        _gamification = gamification;
        _audit = audit;
    }

    public IReadOnlyList<TodoItem> Items => _items;

    public int CompletedTodayCount => _items.Count(i => i.CompletedAt?.Date == DateTime.Today);

    public event Action? OnChange;
    public event Action<TodoItem, CompletionResult>? OnCompleted;

    public TodoItem Add(string text, Priority priority)
    {
        var item = new TodoItem { Text = text, Priority = priority };
        _items.Add(item);
        _audit.Log($"Added objective \"{item.Text}\" [{item.Priority}]");
        OnChange?.Invoke();
        return item;
    }

    public void Edit(TodoItem item, string newText)
    {
        var oldText = item.Text;
        item.Text = newText;
        _audit.Log($"Edited objective \"{oldText}\" to \"{newText}\"");
        OnChange?.Invoke();
    }

    public void Delete(TodoItem item)
    {
        _items.Remove(item);
        _audit.Log($"Deleted objective \"{item.Text}\"");
        OnChange?.Invoke();
    }

    public ToggleResult ToggleDone(TodoItem item)
    {
        item.Done = !item.Done;
        CompletionResult? completion = null;

        if (item.Done)
        {
            item.CompletedAt = DateTime.Now;
            completion = _gamification.RegisterCompletion(item.Priority, item.CompletedAt.Value);
            item.PointsEarned = completion.PointsEarned;
            _audit.Log($"Completed objective \"{item.Text}\" (+{completion.PointsEarned} XP, combo x{completion.Combo})");
            OnCompleted?.Invoke(item, completion);
        }
        else
        {
            item.CompletedAt = null;
            _gamification.RevertCompletion(item.PointsEarned);
            _audit.Log($"Reopened objective \"{item.Text}\" (-{item.PointsEarned} XP)");
            item.PointsEarned = 0;
        }

        OnChange?.Invoke();
        return new ToggleResult(item.Done, completion);
    }

    public void LoadState(IEnumerable<TodoItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        OnChange?.Invoke();
    }
}
