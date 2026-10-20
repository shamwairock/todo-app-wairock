using System.Net.Http.Json;
using TodoApp.Client.Models;

namespace TodoApp.Client.Services;

/// <summary>
/// Talks to the TodoApp.Api backend (Neon Postgres behind EF Core) for Todo item CRUD.
/// Every call is wrapped so a missing/unreachable database degrades to "unavailable"
/// instead of throwing through the UI - useful while the connection string is still
/// being provisioned.
/// </summary>
public class TodoApiClient
{
    private readonly HttpClient _http;

    public TodoApiClient(HttpClient http)
    {
        _http = http;
    }

    public bool IsAvailable { get; private set; } = true;

    public async Task<List<TodoItem>?> TryGetAllAsync()
    {
        try
        {
            var items = await _http.GetFromJsonAsync<List<TodoItemDto>>("api/todoitems");
            IsAvailable = true;
            return items?.Select(FromDto).ToList();
        }
        catch (Exception)
        {
            IsAvailable = false;
            return null;
        }
    }

    public async Task<bool> TryCreateAsync(TodoItem item)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/todoitems", new
            {
                id = item.Id,
                text = item.Text,
                priority = item.Priority.ToString(),
                createdAt = item.CreatedAt,
            });
            IsAvailable = response.IsSuccessStatusCode;
            return IsAvailable;
        }
        catch (Exception)
        {
            IsAvailable = false;
            return false;
        }
    }

    public async Task<bool> TryUpdateAsync(TodoItem item)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/todoitems/{item.Id}", ToDto(item));
            IsAvailable = response.IsSuccessStatusCode;
            return IsAvailable;
        }
        catch (Exception)
        {
            IsAvailable = false;
            return false;
        }
    }

    public async Task<bool> TryDeleteAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/todoitems/{id}");
            IsAvailable = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
            return IsAvailable;
        }
        catch (Exception)
        {
            IsAvailable = false;
            return false;
        }
    }

    private static TodoItem FromDto(TodoItemDto dto) => new()
    {
        Id = dto.Id,
        Text = dto.Text,
        Priority = Enum.Parse<Priority>(dto.Priority),
        Done = dto.Done,
        PointsEarned = dto.PointsEarned,
        CreatedAt = dto.CreatedAt,
        CompletedAt = dto.CompletedAt,
    };

    private static TodoItemDto ToDto(TodoItem item) => new()
    {
        Id = item.Id,
        Text = item.Text,
        Priority = item.Priority.ToString(),
        Done = item.Done,
        PointsEarned = item.PointsEarned,
        CreatedAt = item.CreatedAt,
        CompletedAt = item.CompletedAt,
    };

    private class TodoItemDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = "";
        public string Priority { get; set; } = "Medium";
        public bool Done { get; set; }
        public int PointsEarned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
