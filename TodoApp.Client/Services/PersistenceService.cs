using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp.Client.Models;

namespace TodoApp.Client.Services;

public class PersistenceService
{
    private const string ThemeKey = "todo-theme";
    private const string AuditCollapsedKey = "todo-audit-collapsed";
    private const string StateKey = "todo-state";

    private readonly IJSRuntime _js;

    public PersistenceService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<string> LoadThemeAsync()
    {
        var value = await _js.InvokeAsync<string?>("todoStorage.getItem", ThemeKey);
        return value ?? "dark";
    }

    public Task SaveThemeAsync(string theme) =>
        _js.InvokeVoidAsync("todoStorage.setItem", ThemeKey, theme).AsTask();

    public async Task<bool> LoadAuditCollapsedAsync()
    {
        var value = await _js.InvokeAsync<string?>("todoStorage.getItem", AuditCollapsedKey);
        return value == "1";
    }

    public Task SaveAuditCollapsedAsync(bool collapsed) =>
        _js.InvokeVoidAsync("todoStorage.setItem", AuditCollapsedKey, collapsed ? "1" : "0").AsTask();

    public async Task<SavedState?> LoadStateAsync()
    {
        var json = await _js.InvokeAsync<string?>("todoStorage.getItem", StateKey);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<SavedState>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public Task SaveStateAsync(IEnumerable<TodoItem> items, IEnumerable<AuditEntry> auditEntries, int totalXp)
    {
        var state = new SavedState
        {
            Items = items.ToList(),
            AuditEntries = auditEntries.ToList(),
            TotalXp = totalXp,
        };
        var json = JsonSerializer.Serialize(state);
        return _js.InvokeVoidAsync("todoStorage.setItem", StateKey, json).AsTask();
    }
}

public class SavedState
{
    public List<TodoItem> Items { get; set; } = new();
    public List<AuditEntry> AuditEntries { get; set; } = new();
    public int TotalXp { get; set; }
}
