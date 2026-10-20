namespace TodoApp.Client.Models;

public class TodoItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Text { get; set; } = "";
    public Priority Priority { get; set; } = Priority.Medium;
    public bool Done { get; set; }
    public int PointsEarned { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
}
