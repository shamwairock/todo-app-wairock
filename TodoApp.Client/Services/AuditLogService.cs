using TodoApp.Client.Models;

namespace TodoApp.Client.Services;

public class AuditLogService
{
    private readonly List<AuditEntry> _entries = new();
    public IReadOnlyList<AuditEntry> Entries => _entries;

    public event Action? OnChange;

    public void Log(string message)
    {
        _entries.Insert(0, new AuditEntry(DateTime.Now, message));
        OnChange?.Invoke();
    }

    public void LoadState(IEnumerable<AuditEntry> entries)
    {
        _entries.Clear();
        _entries.AddRange(entries);
        OnChange?.Invoke();
    }
}
